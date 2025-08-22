namespace PodMD.Api.DTOs.Schemas;

public static class TroubleshootingResponseSchema
{
    public static readonly string Value = """
                                          
                                              {
                                            "type": "object",
                                            "properties": {
                                              "errors": {
                                                "type": "array",
                                                "description": "A list of errors with their messages, occurrences, and solutions.",
                                                "items": {
                                                  "type": "object",
                                                  "properties": {
                                                    "general_message": {
                                                      "type": "string",
                                                      "description": "A general description of the error."
                                                    },
                                                    "occurrences": {
                                                      "type": "array",
                                                      "description": "List of places or contexts where the error occurs.",
                                                      "items": {
                                                        "type": "string",
                                                        "description": "A context or occurrence of the error."
                                                      }
                                                    },
                                                    "solutions": {
                                                      "type": "array",
                                                      "description": "Possible solutions to the error, each with a description and guided steps.",
                                                      "items": {
                                                        "type": "object",
                                                        "properties": {
                                                          "description": {
                                                            "type": "string",
                                                            "description": "Description of the solution approach."
                                                          },
                                                          "steps": {
                                                            "type": "array",
                                                            "description": "Step-by-step guide to implement the solution.",
                                                            "items": {
                                                              "type": "object",
                                                              "properties": {
                                                                "title": {
                                                                  "type": "string",
                                                                  "description": "Step title summarizing the action."
                                                                },
                                                                "explanation": {
                                                                  "type": "string",
                                                                  "description": "Explanation of why/how this step helps solve the error."
                                                                },
                                                                "command": {
                                                                  "type": "string",
                                                                  "description": "Optional command to execute for the step."
                                                                }
                                                              },
                                                              "required": [
                                                                "title",
                                                                "explanation",
                                                                "command"
                                                              ],
                                                              "additionalProperties": false
                                                            }
                                                          }
                                                        },
                                                        "required": [
                                                          "description",
                                                          "steps"
                                                        ],
                                                        "additionalProperties": false
                                                      }
                                                    }
                                                  },
                                                  "required": [
                                                    "general_message",
                                                    "occurrences",
                                                    "solutions"
                                                  ],
                                                  "additionalProperties": false
                                                }
                                              }
                                            },
                                            "additionalProperties": false,
                                            "required": [
                                              "errors"
                                            ]
                                          }
                                          """;
}