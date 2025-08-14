namespace PodMD.Api.DTOs.Schemas;

public static class TroubleshootingResponseSchema
{
    public static readonly byte[] Value =
        """
            {
              "type": "object",
              "properties": {
                "errors": {
                  "type": "array",
                  "description": "A list of grouped errors, each representing a unique root cause with all its specific log occurrences.",
                  "items": {
                    "type": "object",
                    "properties": {
                      "general_message": {
                        "type": "string",
                        "description": "A generalized meaning of the error, independent of transient details like IDs or timestamps."
                      },
                      "occurrences": {
                        "type": "array",
                        "description": "List of exact error messages from logs, each potentially multi-line, containing transient values such as IDs or timestamps.",
                        "items": {
                          "type": "object",
                          "properties": {
                            "message": {
                              "type": "string",
                              "description": "Exact error text as found in the logs."
                            }
                          },
                          "required": ["message"],
                          "additionalProperties": false
                        }
                      },
                      "solutions": {
                        "type": "array",
                        "description": "One or more possible troubleshooting solutions for the error.",
                        "items": {
                          "type": "object",
                          "properties": {
                            "description": {
                              "type": "string",
                              "description": "Brief summary of the solution."
                            },
                            "steps": {
                              "type": "array",
                              "description": "List of actionable steps to resolve the error.",
                              "items": {
                                "type": "object",
                                "properties": {
                                  "title": {
                                    "type": "string",
                                    "description": "Short title of the step."
                                  },
                                  "explanation": {
                                    "type": "string",
                                    "description": "Detailed explanation of what the step does and why."
                                  },
                                  "command": {
                                    "type": "string",
                                    "description": "Shell or CLI command to execute the step, if applicable."
                                  }
                                },
                                "required": ["title", "explanation", "command"],
                                "additionalProperties": false
                              }
                            }
                          },
                          "required": ["description", "steps"],
                          "additionalProperties": false
                        }
                      }
                    },
                    "required": ["general_message", "occurrences", "solutions"],
                    "additionalProperties": false
                  }
                }
              },
              "required": ["errors"],
              "additionalProperties": false
            }
            """u8.ToArray();
}