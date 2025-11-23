"""
AI Agent for generating new processes using OpenAI/LangChain
Enhanced with ITIL 4 knowledge base integration for Epic 4
"""
import os
import json
from typing import Dict, Any, List, Callable, Optional
from openai import AsyncOpenAI
from datetime import datetime
from .itil_knowledge_agent import ITILKnowledgeAgent


class ProcessGeneratorAgent:
    """AI Agent that generates new business processes based on requirements"""
    
    def __init__(self):
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            raise ValueError("OPENAI_API_KEY environment variable must be set")
        
        self.client = AsyncOpenAI(api_key=api_key)
        self.model = os.getenv("OPENAI_MODEL", "gpt-4")
        
        # Initialize ITIL knowledge agent for Epic 4
        try:
            self.itil_agent = ITILKnowledgeAgent()
        except Exception as e:
            print(f"Warning: Could not initialize ITIL knowledge agent: {e}")
            self.itil_agent = None
        
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
    
    async def generate_itil_process(self, request_data: Dict[str, Any], progress_callback: Callable[[int, str], None]) -> Dict[str, Any]:
        """
        Generate a new process with ITIL 4 knowledge integration (Epic 4)
        """
        await progress_callback(5, "Initializing ITIL-enhanced generation...")
        
        # Extract request parameters
        title = request_data.get("title", "")
        description = request_data.get("description", "")
        category = request_data.get("category", "")
        itil_area = request_data.get("itil_area", "")
        requirements = request_data.get("requirements", [])
        target_audience = request_data.get("target_audience", "")
        complexity_level = request_data.get("complexity_level", "medium")
        
        await progress_callback(15, "Loading ITIL knowledge base...")
        
        # Get ITIL context if available
        itil_context = {}
        if self.itil_agent and itil_area:
            # Determine process type from title
            process_type = self._determine_itil_process_type(title, description)
            itil_context = self.itil_agent.generate_itil_process_context(process_type, itil_area.lower().replace(" ", "-"))
            
            await progress_callback(25, f"Loaded ITIL context for {itil_area}...")
        
        await progress_callback(35, "Building ITIL-enhanced prompt...")
        
        # Build enhanced prompt with ITIL knowledge
        prompt = self._build_itil_enhanced_prompt(
            title, description, category, itil_area, requirements, 
            target_audience, complexity_level, itil_context
        )
        
        await progress_callback(45, "Calling OpenAI API with ITIL context...")
        
        try:
            # Call OpenAI API with enhanced system message
            system_message = self._build_itil_system_message(itil_context)
            
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {
                        "role": "system",
                        "content": system_message
                    },
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ],
                temperature=0.7,
                max_tokens=3000  # Increased for ITIL-enhanced responses
            )
            
            await progress_callback(75, "Processing ITIL-enhanced response...")
            
            # Parse the response
            ai_response = response.choices[0].message.content
            process_data = self._parse_ai_response(ai_response)
            
            await progress_callback(85, "Validating against ITIL standards...")
            
            # Validate against ITIL if agent available
            validation_results = {}
            if self.itil_agent and itil_area:
                validation_results = self.itil_agent.validate_process_against_itil({
                    "title": title,
                    "description": description,
                    "steps": process_data.get("steps", [])
                })
            
            await progress_callback(95, "Finalizing ITIL-compliant process...")
            
            # Structure the final result with ITIL enhancements
            result = {
                "title": process_data.get("title", title),
                "description": process_data.get("description", description),
                "category": category,
                "itil_area": itil_area,
                "steps": process_data.get("steps", []),
                "estimated_duration": process_data.get("estimated_duration"),
                "tags": process_data.get("tags", []),
                "itil_compliance": validation_results,
                "metadata": {
                    "ai_generated": True,
                    "generation_model": self.model,
                    "generated_at": datetime.utcnow().isoformat(),
                    "itil_enhanced": bool(self.itil_agent and itil_area),
                    "confidence_score": validation_results.get("compliance_score", 0.85),
                    "original_request": {
                        "title": title,
                        "description": description,
                        "category": category,
                        "itil_area": itil_area,
                        "complexity_level": complexity_level
                    }
                }
            }
            
            await progress_callback(100, "ITIL-enhanced process generation completed")
            return result
            
        except Exception as e:
            raise Exception(f"Failed to generate ITIL-enhanced process: {str(e)}")
    
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
    
    def _determine_itil_process_type(self, title: str, description: str) -> str:
        """Determine ITIL process type from title and description"""
        text = f"{title} {description}".lower()
        
        if any(keyword in text for keyword in ["incident", "issue", "outage", "disruption"]):
            return "incident-management"
        elif any(keyword in text for keyword in ["change", "modification", "update", "upgrade"]):
            return "change-management" 
        elif any(keyword in text for keyword in ["problem", "root cause", "recurring"]):
            return "problem-management"
        elif any(keyword in text for keyword in ["request", "service request", "fulfillment"]):
            return "service-request-management"
        elif any(keyword in text for keyword in ["access", "permission", "authorization"]):
            return "access-management"
        else:
            # Default to incident management for unknown types
            return "incident-management"
    
    def _build_itil_system_message(self, itil_context: Dict[str, Any]) -> str:
        """Build ITIL-enhanced system message"""
        
        base_message = "Du er en ekspert på ITIL 4 og norsk forretningsprosess-design. Du lager detaljerte, strukturerte og praktiske forretningsprosesser som følger ITIL 4 beste praksis."
        
        if not itil_context:
            return base_message
            
        # Add ITIL-specific guidance
        guiding_principles = itil_context.get("service_value_system", {}).get("guiding_principles", [])
        principles_text = ", ".join([p.get("name", "") for p in guiding_principles[:3]])
        
        enhanced_message = f"""{base_message}

ITIL 4 KONTEKST:
- Følg ITIL 4 Service Value System prinsipper: {principles_text}
- Integrer med Service Value Chain aktiviteter
- Sørg for at prosessen er i tråd med norsk forretningskultur
- Inkluder relevante roller, ansvar og eskaleringsveier
- Legg vekt på kontinuerlig forbedring og måling

NORSK KONTEKST:
- Kommunikasjon på norsk
- Direkte og transparent kommunikasjonsstil  
- Konsensusbasert beslutningstaking
- GDPR-compliance

Lag prosesser som er praktiske, målbare og i tråd med ITIL 4 standarder."""

        return enhanced_message
    
    def _build_itil_enhanced_prompt(self, title: str, description: str, category: str, 
                                  itil_area: str, requirements: List[str], target_audience: str, 
                                  complexity_level: str, itil_context: Dict[str, Any]) -> str:
        """Build ITIL-enhanced prompt with knowledge base integration"""
        
        # Get specific practice knowledge if available
        practice_specific = itil_context.get("process_specific", {})
        
        # Extract key activities and metrics
        key_activities = practice_specific.get("key_activities", [])
        key_metrics = practice_specific.get("key_metrics", [])
        
        # Build activities text
        activities_text = ""
        if key_activities:
            activities_text = "\n**ITIL Nøkkelaktiviteter for referanse:**\n"
            for activity in key_activities[:5]:  # Limit to 5
                activities_text += f"- {activity.get('activity', '')}: {activity.get('description', '')}\n"
        
        # Build metrics text
        metrics_text = ""
        if key_metrics:
            metrics_text = "\n**Relevante KPI-er:**\n"
            for metric in key_metrics[:3]:  # Limit to 3
                metrics_text += f"- {metric.get('metric', '')}: {metric.get('description', '')}\n"
        
        prompt = f"""
Lag en detaljert ITIL 4-kompatibel forretningsprosess med følgende spesifikasjoner:

**Prosessinformasjon:**
- Tittel: {title}
- Beskrivelse: {description}
- Kategori: {category}
- ITIL-område: {itil_area}
- Målgruppe: {target_audience}
- Kompleksitetsnivå: {complexity_level}

**Krav:**
{chr(10).join(f"- {req}" for req in requirements) if requirements else "- Ingen spesifikke krav oppgitt"}

{activities_text}

{metrics_text}

Lag en omfattende prosess som inkluderer:

1. **Prosessoversikt:**
   - Raffinert tittel (hvis nødvendig)
   - Detaljert beskrivelse med ITIL-kontekst
   - Formål og målsettinger i tråd med ITIL 4

2. **Prosesstrinn:**
   Lag 5-12 logiske, sekvensielle trinn basert på ITIL beste praksis. For hvert trinn:
   - Trinntittel (klar og handlingsorientert)
   - Detaljert beskrivelse
   - Type (Task, Decision, Document, Approval, etc.)
   - Ansvarlig rolle/avdeling (bruk ITIL-roller)
   - Estimert varighet i minutter
   - Eventuelle forutsetninger eller avhengigheter
   - Valgfritt: under-trinn hvis nødvendig

3. **ITIL-integrasjon:**
   - Kobling til relevante ITIL-praksiser
   - Integrasjon med Service Value Chain
   - Eskaleringsveier og beslutningspunkter
   - Roller og ansvar i henhold til ITIL

4. **Norsk forretningskontekst:**
   - Tilpasning til norske forretningsmetoder
   - Kommunikasjon på norsk
   - Juridiske og regulatoriske hensyn

5. **Måling og forbedring:**
   - KPI-er og målinger
   - Kontinuerlig forbedringsmuligheter

Svar i JSON-format:
```json
{{
  "title": "Prosesstittel",
  "description": "Detaljert prosessbeskrivelse med ITIL-kontekst",
  "estimated_duration": 180,
  "tags": ["itil", "service-operation", "norsk"],
  "itil_practices": ["relevant ITIL-praksis"],
  "steps": [
    {{
      "title": "Trinntittel",
      "description": "Detaljert trinnbeskrivelse", 
      "type": "Task",
      "responsible_role": "ITIL-rolle",
      "estimated_duration": 30,
      "order_index": 1,
      "is_optional": false,
      "detailed_instructions": "Spesifikke instruksjoner for trinnet",
      "itil_guidance": "ITIL beste praksis for dette trinnet"
    }}
  ]
}}
```

Lag prosessen praktisk, realistisk og godt strukturert med fokus på ITIL 4 compliance og norsk forretningspraksis.
"""
        return prompt.strip()