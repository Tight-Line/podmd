"""
PodMD Python Backend - LLM Client
OpenAI client for log analysis.
"""

import logging
import json
from typing import Optional, Dict, Any
from asyncio import sleep

import openai
from openai import AsyncOpenAI

from app.schemas.analysis import Solution, Step, LogError

logger = logging.getLogger(__name__)


class LLMClient:
    """OpenAI client for Kubernetes log analysis."""

    def __init__(self, api_key: str, model: str = "gpt-3.5-turbo"):
        self.client = AsyncOpenAI(api_key=api_key)
        self.model = model
        # Hard-coded defaults to match .NET backend
        self.troubleshooting_prompt = (
            "You are a Kubernetes troubleshooting assistant. " +
            "You will be given the latest logs from a pod in the next message, which may contain multi-line errors.\n\n" +
            "INSTRUCTIONS:\n" +
            "1) Identify all unique root-cause errors. Use the full log including stack traces, but do NOT include stack traces in the output.\n" +
            "2) Group all occurrences of each root-cause error under it. For each occurrence, include only the main error message (omit stack trace).\n" +
            "3) Deduplicate by ignoring timestamps, connection IDs, request IDs, and other transient values.\n" +
            "4) For each group, output: \n" +
            "   - general_message: a generalized meaning of the error,\n" +
            "   - occurrences: list of exact error messages from logs (multi-line if needed),\n" +
            "   - solutions: one or more troubleshooting solutions with {description, steps (title, explanation, command if available)}.\n" +
            "5) Do NOT include warnings (lines with 'warn', 'warning', 'WARN', 'WARNING').\n\n" +
            "6) Utilize Uploaded Files: Before finalizing the output, explicitly search through any uploaded files for documentation that matches the errors identified in the logs. Cross-reference these documents for detailed troubleshooting steps.\n" +
            "7) Contextual Matching: Ensure each solution is contextually aligned with both the log errors and any additional documentation available in the uploaded files to improve accuracy and relevancy.\n" +
            "8) Iterative Analysis: After an initial solution set is drafted, refine solutions iteratively by integrating insights found from the user's documentation to provide comprehensive and contextual troubleshooting guidance.\n" +
            "9) Feedback and Enhancement: Continuously improve the output by reflecting on any mismatches or missed opportunities between the log errors and solutions from uploaded files for future tasks.\n" +
            "OUTPUT RULES:\n" +
            "- Output ONLY valid JSON.\n" +
            "- Do NOT add any explanations or text outside the JSON.\n" +
            "- Use ParseTroubleshootingLogs function."
        )

        self.default_response_format = '''{
  "errors": [
    {
      "description": "short description of the error",
      "occurrences": ["original error line 1", "original error line 2"],
      "solutions": [
        {
          "description": "solution description",
          "steps": [
            {
              "title": "step title",
              "explanation": "step explanation",
              "command": "kubectl or helm command"
            }
          ]
        }
      ]
    }
  ]
}'''

    async def analyze_logs_async(
        self,
        logs: str,
        instructions: Optional[str] = None,
        description: Optional[str] = None,
        max_retries: int = 3
    ) -> str:
        """
        Analyze Kubernetes logs using OpenAI API.

        Args:
            logs: The raw logs to analyze
            instructions: Custom instructions (defaults to troubleshooting prompt)
            description: Resource description for context
            max_retries: Maximum retry attempts on API failures

        Returns:
            JSON string response from OpenAI
        """
        prompt = instructions or self.troubleshooting_prompt

        # Build the complete prompt
        messages = []
        if description:
            messages.append({"role": "system", "content": f"Resource Context:\n{description}"})
        messages.append({"role": "user", "content": f"{prompt}\n\nLogs:\n{logs}\n\nReturn a JSON object in this format:\n{self.default_response_format}"})

        for attempt in range(max_retries):
            try:
                logger.info(f"Sending analysis request to OpenAI (attempt {attempt + 1})")
                logger.info(f"Request logs: {logs[:500]}{'...' if len(logs) > 500 else ''}")
                logger.info(f"Request description: {description[:200]}{'...' if description and len(description) > 200 else ''}")

                response = await self.client.chat.completions.create(
                    model=self.model,
                    messages=messages,
                    max_tokens=2000,
                    temperature=0.1  # Low temperature for consistent troubleshooting
                )

                result = response.choices[0].message.content
                if result:
                    logger.info("Successfully received response from OpenAI")
                    logger.info(f"LLM Response content: {result[:500]}{'...' if len(result) > 500 else ''}")
                    return result

            except Exception as e:
                logger.warning(f"OpenAI API call failed (attempt {attempt + 1}): {str(e)}")
                if attempt < max_retries - 1:
                    await sleep(2 ** attempt)  # Exponential backoff
                else:
                    raise Exception(f"LLM analysis failed after {max_retries} attempts: {str(e)}")

        raise Exception("LLM analysis failed: No response received")

    def parse_analysis_response(self, response: str) -> Dict[str, Any]:
        """
        Parse the JSON response from OpenAI into structured format.

        Returns the raw JSON object for now, following .NET backend pattern.
        """
        cleaned_response = self._clean_llm_response(response)
        try:
            return json.loads(cleaned_response)
        except json.JSONDecodeError as e:
            logger.error(f"Failed to parse LLM response: {e}")
            raise ValueError("Failed to parse LLM response as JSON")

    def _clean_llm_response(self, response: str) -> str:
        """Clean the LLM response to extract JSON."""
        if not response:
            return response

        # Try to extract JSON from markdown code blocks
        import re
        json_block_pattern = r'```(?:json)?\s*(\{[\s\S]*?\})\s*```'
        match = re.search(json_block_pattern, response, re.IGNORECASE | re.DOTALL)
        if match:
            return match.group(1).strip()

        # Fallback: look for the first { and last }
        start_index = response.find('{')
        last_index = response.rfind('}')

        if start_index >= 0 and last_index > start_index:
            extracted = response[start_index:last_index + 1].strip()
            # Remove leading/trailing quotes/backticks if present
            extracted = extracted.strip('"`\'\n\r\t ')
            return extracted

        # Return original if no JSON found (will fail JSON parse and be handled)
        return response.strip()
