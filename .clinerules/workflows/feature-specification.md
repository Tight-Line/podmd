# Feature Specification & Task Generation Workflow

**Progress: Step 1 of 8** | ▰▰▰▰▰▰▰▰▱▱ | 12.5% Complete

You are following custom Cline instructions.  
This workflow creates a comprehensive feature specification and generates an implementation-ready task file.  
The main deliverable is a detailed markdown task file containing all requirements, specifications, and implementation guidance.

---

## Step 1 – Confirm Objective

<ask_followup_question>
<question>
What’s the objective of this task?  
(Describe what the feature/module should achieve, including encryption, security, or metadata rules if needed.)
</question>
<options>["Provide objective", "Cancel workflow"]</options>
</ask_followup_question>

---

## Step 1 Confirmation

<ask_followup_question>
<question>
Based on your objective description:

**[User's objective will be summarized here]**

**Key Points:**

- Main goal identified
- Success criteria noted
- Any special requirements (security, encryption, etc.) highlighted

Does this accurately capture your feature objective? You can proceed to scope definition, revise the objective, or cancel the workflow.
</question>
<options>["Proceed to Step 2", "Revise Step 1", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 2 of 8** | ▰▰▰▰▰▰▰▰▰▱ | 25% Complete

## Step 2 – Define Scope

<ask_followup_question>
<question>
Please define the scope.  
What must be included (entities, DTOs, endpoints, validation/security rules)?  
What should be excluded (non-essential features, other auth types, extended functionality)?
</question>
<options>["Provide scope", "Cancel workflow"]</options>
</ask_followup_question>

---

## Step 2 Confirmation

<ask_followup_question>
<question>
Based on your scope definition:

**[User's scope will be summarized here]**

**Key Points:**

- Must-include features identified
- Excluded features clarified
- Technical boundaries established
- Success criteria aligned with objective

Does this accurately represent your feature scope? You can proceed to entity design, revise the scope, or go back to the objective.
</question>
<options>["Proceed to Step 3", "Revise Step 2", "Go back to Step 1", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 3 of 8** | ▰▰▰▰▰▰▰▰▰▰ | 37.5% Complete

## Step 3 – Entities

<ask_followup_question>
<question>
Describe the entity fields you need in plain English. For example:

- id (Guid, required) - Primary key
- name (string, required) - User's full name
- email (string, required, unique) - User's email address
- createdAt (DateTime, auto) - When the record was created

Include data types, whether fields are required/optional, and any special constraints (unique, encrypted, etc.).
</question>
<options>["Provide entity descriptions", "Skip"]</options>
</ask_followup_question>

<new_task>
Generate a professional entity fields table from the user's descriptions, including standard audit fields and common constraints. Check existing database schema to avoid conflicts with existing tables.
</new_task>

<ask_followup_question>
<question>
Based on your entity descriptions, here's the generated table:

**Entity Fields Table:**
[Generated table will show fields like: id, name, email, etc.]

**Key Points:**

- Primary key field identified
- Required vs optional fields marked
- Data types and constraints applied
- Standard audit fields (CreatedAt, UpdatedAt) included

Does this table accurately represent your entity requirements? You can proceed, revise the descriptions, or go back to adjust scope.
</question>
<options>["Yes, proceed with this table", "Revise entity descriptions", "Go back to previous step", "Cancel workflow"]</options>
</ask_followup_question>

---

## Step 3 Confirmation

<ask_followup_question>
<question>
Are you satisfied with the entity design? Would you like to proceed to DTOs, revise entities, or go back to scope?
</question>
<options>["Proceed to Step 4", "Revise Step 3", "Go back to Step 2", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 4 of 8** | ▰▰▰▰▰▰▰▰▰▰ | 50% Complete

## Step 4 – DTOs

<ask_followup_question>
<question>
Define DTOs for Create, Update, and Read.  
Mark fields as required/optional and note special handling (e.g. exclude secrets in Read).
</question>
<options>["Provide DTOs", "Skip"]</options>
</ask_followup_question>

---

## Step 4 Confirmation

<ask_followup_question>
<question>
Based on your DTO definitions:

**[User's DTO specifications will be summarized here]**

**Key Points:**

- Create/Update/Read DTOs defined
- Required vs optional fields specified
- Special handling noted (password exclusion, etc.)
- Validation rules identified

Does this accurately represent your DTO requirements? You can proceed to API endpoints, revise DTOs, or go back to entities.
</question>
<options>["Proceed to Step 5", "Revise Step 4", "Go back to Step 3", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 5 of 8** | ▰▰▰▰▰▰▰▰▰▰ | 62.5% Complete

## Step 5 – API Endpoints

<ask_followup_question>
<question>
Describe the API endpoints you need in plain English. For example:

- POST /api/users - Create a new user
- GET /api/users/{id} - Get user by ID
- PUT /api/users/{id} - Update user information
- DELETE /api/users/{id} - Delete user

Include any specific constraints (validation, auth requirements, etc.) if known.
</question>
<options>["Provide endpoint descriptions", "Skip"]</options>
</ask_followup_question>

<new_task>
Generate a professional API endpoints table from the user's descriptions, including standard REST patterns and common constraints.
</new_task>

<ask_followup_question>
<question>
Based on your endpoint descriptions, here's the generated table:

**API Endpoints Table:**
[Generated table will show methods, paths, descriptions]

**Key Points:**

- RESTful HTTP methods identified (GET, POST, PUT, DELETE)
- Consistent URL patterns applied
- Authentication requirements noted
- Response types and status codes included

Does this table accurately represent your API requirements? You can proceed, revise the descriptions, or go back to adjust DTOs.
</question>
<options>["Yes, proceed with this table", "Revise endpoint descriptions", "Go back to previous step", "Cancel workflow"]</options>
</ask_followup_question>

---

## Step 5 Confirmation

<ask_followup_question>
<question>
Are you satisfied with the API endpoint definitions? Would you like to proceed to file planning, revise endpoints, or go back to DTOs?
</question>
<options>["Proceed to Step 6", "Revise Step 5", "Go back to Step 4", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 6 of 8** | ▰▰▰▰▰▰▰▰▰▰ | 75% Complete

## Step 6 – Files

<ask_followup_question>
<question>
List the files (descriptions only) that should be created/modified, for example:

- `Entities/[Entity].cs` → entity definition
- `Dtos/[Entity]Dtos.cs` → DTOs
- `Controllers/[Entity]Controller.cs` → endpoints
- `Infrastructure/Encryption.cs` → converters
- `Data/Migrations/[Entity]` → EF config
- `Program.cs` → DI/Swagger setup
  </question>
  <options>["Provide file paths", "Skip"]</options>
  </ask_followup_question>

---

## Step 6 Confirmation

<ask_followup_question>
<question>
Based on your file planning:

**[User's file list will be summarized here]**

**Key Points:**

- Entity and DTO files identified
- Controller and service files planned
- Database migration files included
- Configuration updates noted

Does this accurately represent the files needed for implementation? You can proceed to task generation, revise files, or go back to endpoints.
</question>
<options>["Proceed to Step 7", "Revise Step 6", "Go back to Step 5", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 7 of 8** | ▰▰▰▰▰▰▰▰▰▰ | 87.5% Complete

## Step 7 – Task File Generation

<ask_followup_question>
<question>
Ready to generate the implementation task file! Where would you like to save the comprehensive task specification?
</question>
<options>["Provide file path for task", "Use default location", "Cancel workflow"]</options>
</ask_followup_question>

<new_task>

1. Leverage accumulated project knowledge from the workflow (memory-bank context, code patterns, existing architecture).
2. Synthesize all gathered specifications into a comprehensive implementation task file:
   - Complete objective and scope definition
   - Entity specifications and database schema
   - DTO definitions and validation rules
   - API endpoint specifications with constraints
   - File structure and implementation guidance
   - All validated assumptions and requirements
3. Save the task file to the specified location.
   </new_task>

<ask_followup_question>
<question>
Task file created successfully! What would you like to do next?
</question>
<options>["Start implementing the task now", "Review the task file first", "Create another feature task", "Exit workflow"]</options>
</ask_followup_question>

---

## Step 7 Confirmation

<ask_followup_question>
<question>
The task file has been generated successfully. Before finalizing, would you like to do a final assumptions check, or proceed directly to completion?
</question>
<options>["Proceed to final assumptions check", "Skip assumptions check and complete", "Cancel workflow"]</options>
</ask_followup_question>

---

**Progress: Step 8 of 8** | ▰▰▰▰▰▰▰▰▰▰ | 100% Complete

## Step 8 – Final Assumptions Check

<loop>
# Step 8a – List Current Assumptions & Uncertainties
<new_task>
1. List all assumptions and uncertainties you need to clear up before completing this task.
</new_task>

<ask_followup_question>
<question>
Provide answers or clarifications for the assumptions/uncertainties listed above.
</question>
<options>["Provide clarifications", "Cancel workflow"]</options>
</ask_followup_question>

# Step 8b – Recheck

<new_task>

1. Re-list only the assumptions and uncertainties that are still unresolved after the user's clarifications.
2. If none remain, exit the loop and proceed to completion.
   </new_task>
   </loop>

---

## Workflow Complete ✅

**🎉 Task File Generated Successfully!**

You have completed the **Feature Specification & Task Generation Workflow**. The main deliverable - a comprehensive, implementation-ready task file - has been created with:

**📋 Complete Specification:**

- **Objective & Scope:** Clearly defined requirements and boundaries
- **Entity Design:** Database schema with relationships and constraints
- **DTO Specifications:** Data transfer objects for all operations
- **API Endpoints:** Complete REST API with validation and security
- **File Structure:** All required code files and modifications
- **Implementation Guidance:** Step-by-step development instructions

**🚀 Ready for Implementation:**
The generated task file contains everything needed to implement the feature:

- Detailed technical specifications
- Code structure and patterns
- Validation rules and constraints
- Database schema changes
- API documentation

**Next Steps:**

- **Start implementing** the feature using the task file as your guide
- **Review the specifications** if you need to make any adjustments
- **Create additional features** using this same workflow
- **Share the task file** with your development team

**The feature is now fully specified and ready for development!** 🎯

**After this step:**  
List all assumptions and uncertainties you need to clear up before completing this task.
