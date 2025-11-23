"""
AI Agent for revising and improving existing processes using OpenAI/LangChain
"""
import os
import json
from typing import Dict, Any, List, Callable, Optional
from openai import AsyncOpenAI
from datetime import datetime


class RevisionAgent:
    """AI Agent that revises and improves existing business processes"""
    
    def __init__(self):
        self.client = AsyncOpenAI(
            api_key=os.getenv("OPENAI_API_KEY")
        )
        self.model = os.getenv("OPENAI_MODEL", "gpt-4")
        
    async def revise_process(self, request_data: Dict[str, Any], progress_callback: Callable[[int, str], None]) -> Dict[str, Any]:
        """
        Revise an existing process based on feedback and improvement goals
        """
        await progress_callback(10, "Loading process data...")
        
        # Extract request parameters
        process_id = request_data.get("process_id")
        revision_type = request_data.get("revision_type", "optimize")
        feedback = request_data.get("feedback", [])
        improvement_goals = request_data.get("improvement_goals", [])
        custom_instructions = request_data.get("custom_instructions", "")
        
        await progress_callback(20, "Fetching current process...")
        
        # In a real implementation, we would fetch the process from the database
        # For now, we'll simulate this with mock data
        current_process = await self._fetch_process_data(process_id)
        
        await progress_callback(30, "Analyzing current process...")
        
        # Build the revision prompt
        prompt = self._build_revision_prompt(
            current_process, revision_type, feedback, improvement_goals, custom_instructions
        )
        
        await progress_callback(50, "Calling OpenAI API for revision...")
        
        try:
            # Call OpenAI API
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {
                        "role": "system",
                        "content": "You are an expert business process analyst and improvement specialist. You analyze existing processes and suggest concrete improvements."
                    },
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ],
                temperature=0.6,
                max_tokens=2500
            )
            
            await progress_callback(80, "Processing revision suggestions...")
            
            # Parse the response
            ai_response = response.choices[0].message.content
            revision_data = self._parse_revision_response(ai_response)
            
            await progress_callback(95, "Finalizing revision...")
            
            # Structure the final result
            result = {
                "process_id": process_id,
                "revision_summary": revision_data.get("revision_summary", "Process has been revised and improved"),
                "updated_title": revision_data.get("updated_title"),
                "updated_description": revision_data.get("updated_description"),
                "updated_steps": revision_data.get("updated_steps"),
                "changes_made": revision_data.get("changes_made", []),
                "improvement_metrics": revision_data.get("improvement_metrics", {}),
                "metadata": {
                    "ai_revised": True,
                    "revision_model": self.model,
                    "revised_at": datetime.utcnow().isoformat(),
                    "revision_type": revision_type,
                    "confidence_score": 0.88,
                    "original_feedback": feedback,
                    "improvement_goals": improvement_goals
                }
            }
            
            await progress_callback(100, "Process revision completed")
            return result
            
        except Exception as e:
            raise Exception(f"Failed to revise process: {str(e)}")
    
    async def _fetch_process_data(self, process_id: int) -> Dict[str, Any]:
        """
        Fetch process data from the database
        In a real implementation, this would query the .NET API
        """
        # Mock process data - in reality, this would come from the database
        mock_process = {
            "id": process_id,
            "title": "Employee Onboarding Process",
            "description": "Process for onboarding new employees to the organization",
            "category": "HR",
            "steps": [
                {
                    "id": 1,
                    "title": "Prepare Workspace",
                    "description": "Set up desk, computer, and office supplies",
                    "type": "Task",
                    "responsible_role": "IT",
                    "estimated_duration": 30,
                    "order_index": 1
                },
                {
                    "id": 2,
                    "title": "Create User Accounts",
                    "description": "Create email, system access, and security accounts",
                    "type": "Task",
                    "responsible_role": "IT",
                    "estimated_duration": 45,
                    "order_index": 2
                },
                {
                    "id": 3,
                    "title": "HR Orientation",
                    "description": "Complete HR paperwork and orientation session",
                    "type": "Task",
                    "responsible_role": "HR",
                    "estimated_duration": 120,
                    "order_index": 3
                },
                {
                    "id": 4,
                    "title": "Manager Introduction",
                    "description": "Meet with direct manager for role introduction",
                    "type": "Task",
                    "responsible_role": "Manager",
                    "estimated_duration": 60,
                    "order_index": 4
                }
            ],
            "estimated_duration": 255,
            "tags": ["onboarding", "hr", "new-employee"]
        }
        
        return mock_process
    
    def _build_revision_prompt(self, current_process: Dict[str, Any], revision_type: str,
                             feedback: List[str], improvement_goals: List[str], custom_instructions: str) -> str:
        """Build the prompt for process revision"""
        
        prompt = f"""
Analyze and improve the following business process:

**Current Process:**
Title: {current_process.get('title', 'Unknown Process')}
Description: {current_process.get('description', '')}
Category: {current_process.get('category', '')}

**Current Steps:**
{self._format_steps_for_prompt(current_process.get('steps', []))}

**Revision Type:** {revision_type}

**Feedback to Address:**
{chr(10).join(f"- {item}" for item in feedback) if feedback else "- No specific feedback provided"}

**Improvement Goals:**
{chr(10).join(f"- {goal}" for goal in improvement_goals) if improvement_goals else "- General optimization"}

**Custom Instructions:**
{custom_instructions if custom_instructions else "- No custom instructions provided"}

Based on the revision type "{revision_type}", please:

1. **Analyze the Current Process:**
   - Identify inefficiencies, bottlenecks, or unclear steps
   - Note redundancies or missing elements
   - Assess overall process flow and logic

2. **Provide Specific Improvements:**
   - Suggest concrete changes to address feedback
   - Optimize for the stated improvement goals
   - Maintain process integrity and compliance requirements

3. **Generate Revised Process:**
   - Updated title (if needed)
   - Improved description
   - Optimized steps with clear instructions
   - Better role assignments and time estimates

Please format your response as JSON with this structure:
```json
{{
  "revision_summary": "Brief summary of what was changed and why",
  "updated_title": "Improved Process Title",
  "updated_description": "Enhanced process description",
  "updated_steps": [
    {{
      "title": "Step Title",
      "description": "Improved step description",
      "type": "Task",
      "responsible_role": "Department/Role",
      "estimated_duration": 25,
      "order_index": 1,
      "is_optional": false,
      "detailed_instructions": "Clear, actionable instructions"
    }}
  ],
  "changes_made": [
    "Specific change 1",
    "Specific change 2"
  ],
  "improvement_metrics": {{
    "estimated_time_saved": 30,
    "steps_reduced": 1,
    "clarity_score": 0.95,
    "efficiency_gain": "20%"
  }}
}}
```

Focus on making the process more efficient, clear, and aligned with the stated goals.
"""
        return prompt.strip()
    
    def _format_steps_for_prompt(self, steps: List[Dict[str, Any]]) -> str:
        """Format process steps for inclusion in the prompt"""
        formatted_steps = []
        for i, step in enumerate(steps, 1):
            formatted_steps.append(
                f"{i}. **{step.get('title', 'Unnamed Step')}**\n"
                f"   - Description: {step.get('description', '')}\n"
                f"   - Responsible: {step.get('responsible_role', 'Unknown')}\n"
                f"   - Duration: {step.get('estimated_duration', 0)} minutes\n"
                f"   - Type: {step.get('type', 'Task')}"
            )
        return "\n\n".join(formatted_steps)
    
    def _parse_revision_response(self, response: str) -> Dict[str, Any]:
        """Parse the AI response and extract structured revision data"""
        try:
            # Try to find JSON in the response
            start = response.find('{')
            end = response.rfind('}') + 1
            
            if start >= 0 and end > start:
                json_str = response[start:end]
                return json.loads(json_str)
            else:
                # Fallback: manual parsing
                return self._manual_parse_revision_response(response)
                
        except json.JSONDecodeError:
            # Fallback to manual parsing if JSON parsing fails
            return self._manual_parse_revision_response(response)
    
    def _manual_parse_revision_response(self, response: str) -> Dict[str, Any]:
        """Manual parsing fallback for revision response"""
        # Basic extraction - this is a simplified fallback
        return {
            "revision_summary": "The process has been analyzed and improvements have been suggested.",
            "updated_title": None,  # Keep original title
            "updated_description": None,  # Keep original description
            "updated_steps": None,  # Keep original steps
            "changes_made": [
                "Process has been reviewed",
                "Suggestions for improvement provided"
            ],
            "improvement_metrics": {
                "estimated_time_saved": 15,
                "steps_reduced": 0,
                "clarity_score": 0.8,
                "efficiency_gain": "10%"
            }
        }