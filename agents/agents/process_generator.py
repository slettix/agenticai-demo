"""
AI Agent for generating new processes using OpenAI/LangChain
"""
import os
import json
from typing import Dict, Any, List, Callable, Optional
from openai import AsyncOpenAI
from datetime import datetime


class ProcessGeneratorAgent:
    """AI Agent that generates new business processes based on requirements"""
    
    def __init__(self):
        self.client = AsyncOpenAI(
            api_key=os.getenv("OPENAI_API_KEY")
        )
        self.model = os.getenv("OPENAI_MODEL", "gpt-4")
        
    async def generate_process(self, request_data: Dict[str, Any], progress_callback: Callable[[int, str], None]) -> Dict[str, Any]:
        """
        Generate a new process based on the request data
        """
        await progress_callback(10, "Analyzing requirements...")
        
        # Extract request parameters
        title = request_data.get("title", "")
        description = request_data.get("description", "")
        category = request_data.get("category", "")
        requirements = request_data.get("requirements", [])
        target_audience = request_data.get("target_audience", "")
        complexity_level = request_data.get("complexity_level", "medium")
        
        await progress_callback(20, "Preparing AI prompt...")
        
        # Build the prompt
        prompt = self._build_generation_prompt(
            title, description, category, requirements, target_audience, complexity_level
        )
        
        await progress_callback(30, "Calling OpenAI API...")
        
        try:
            # Call OpenAI API
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {
                        "role": "system",
                        "content": "You are an expert business process designer. You create detailed, structured, and practical business processes."
                    },
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ],
                temperature=0.7,
                max_tokens=2500
            )
            
            await progress_callback(70, "Processing AI response...")
            
            # Parse the response
            ai_response = response.choices[0].message.content
            process_data = self._parse_ai_response(ai_response)
            
            await progress_callback(90, "Structuring process data...")
            
            # Structure the final result
            result = {
                "title": process_data.get("title", title),
                "description": process_data.get("description", description),
                "category": category,
                "steps": process_data.get("steps", []),
                "estimated_duration": process_data.get("estimated_duration"),
                "tags": process_data.get("tags", []),
                "metadata": {
                    "ai_generated": True,
                    "generation_model": self.model,
                    "generated_at": datetime.utcnow().isoformat(),
                    "confidence_score": 0.85,  # Could be computed from response
                    "original_request": {
                        "title": title,
                        "description": description,
                        "category": category,
                        "complexity_level": complexity_level
                    }
                }
            }
            
            await progress_callback(100, "Process generation completed")
            return result
            
        except Exception as e:
            raise Exception(f"Failed to generate process: {str(e)}")
    
    def _build_generation_prompt(self, title: str, description: str, category: str, 
                               requirements: List[str], target_audience: str, complexity_level: str) -> str:
        """Build the prompt for process generation"""
        
        prompt = f"""
Generate a detailed business process with the following specifications:

**Process Title:** {title}
**Description:** {description}
**Category:** {category}
**Target Audience:** {target_audience}
**Complexity Level:** {complexity_level}

**Requirements:**
{chr(10).join(f"- {req}" for req in requirements) if requirements else "- No specific requirements provided"}

Please generate a comprehensive process that includes:

1. **Process Overview:**
   - Refined title (if needed)
   - Detailed description
   - Purpose and objectives

2. **Process Steps:**
   Create 5-15 logical, sequential steps. For each step, provide:
   - Step title (clear and action-oriented)
   - Detailed description
   - Type (Task, Decision, Document, Approval, etc.)
   - Responsible role/department
   - Estimated duration in minutes
   - Any prerequisites or dependencies
   - Optional: sub-steps if needed

3. **Process Metadata:**
   - Estimated total duration
   - Suggested tags for categorization
   - Key success metrics

Please format your response as JSON with this structure:
```json
{{
  "title": "Process Title",
  "description": "Detailed process description",
  "estimated_duration": 120,
  "tags": ["tag1", "tag2", "tag3"],
  "steps": [
    {{
      "title": "Step Title",
      "description": "Detailed step description", 
      "type": "Task",
      "responsible_role": "Department/Role",
      "estimated_duration": 30,
      "order_index": 1,
      "is_optional": false,
      "detailed_instructions": "Specific instructions for this step"
    }}
  ]
}}
```

Make the process practical, realistic, and well-structured. Consider industry best practices and ensure logical flow between steps.
"""
        return prompt.strip()
    
    def _parse_ai_response(self, response: str) -> Dict[str, Any]:
        """Parse the AI response and extract structured process data"""
        try:
            # Try to find JSON in the response
            start = response.find('{')
            end = response.rfind('}') + 1
            
            if start >= 0 and end > start:
                json_str = response[start:end]
                return json.loads(json_str)
            else:
                # Fallback: manual parsing
                return self._manual_parse_response(response)
                
        except json.JSONDecodeError:
            # Fallback to manual parsing if JSON parsing fails
            return self._manual_parse_response(response)
    
    def _manual_parse_response(self, response: str) -> Dict[str, Any]:
        """Manual parsing fallback if JSON parsing fails"""
        lines = response.strip().split('\n')
        
        # Basic extraction - this is a simplified fallback
        process_data = {
            "title": "Generated Process",
            "description": "AI-generated business process",
            "estimated_duration": 60,
            "tags": ["ai-generated"],
            "steps": []
        }
        
        current_step = None
        step_counter = 1
        
        for line in lines:
            line = line.strip()
            
            if line.startswith('**') and 'title' in line.lower():
                # Extract title
                title_match = line.split(':', 1)
                if len(title_match) > 1:
                    process_data["title"] = title_match[1].strip().strip('*')
            
            elif line.startswith('**') and 'description' in line.lower():
                # Extract description
                desc_match = line.split(':', 1)
                if len(desc_match) > 1:
                    process_data["description"] = desc_match[1].strip().strip('*')
            
            elif line.startswith(str(step_counter) + '.') or line.startswith('- '):
                # This looks like a step
                step_title = line.lstrip('0123456789.- ').strip()
                if step_title:
                    step = {
                        "title": step_title,
                        "description": step_title,
                        "type": "Task",
                        "responsible_role": "Team Member",
                        "estimated_duration": 15,
                        "order_index": step_counter,
                        "is_optional": False,
                        "detailed_instructions": f"Complete the task: {step_title}"
                    }
                    process_data["steps"].append(step)
                    step_counter += 1
        
        return process_data