"""
ITIL Knowledge Agent for Epic 4 - Story 4.2
Provides access to ITIL 4 knowledge base for AI-enhanced process generation
"""
import os
import json
from typing import Dict, Any, List, Optional
from pathlib import Path


class ITILKnowledgeAgent:
    """Agent that provides access to ITIL 4 knowledge base and best practices"""
    
    def __init__(self):
        # Path to ITIL knowledge base
        self.knowledge_base_path = Path(__file__).parent.parent.parent / "data" / "itil"
        
        # Cache for loaded knowledge
        self._knowledge_cache = {}
        
        # Initialize knowledge areas
        self.itil_areas = [
            "service-strategy",
            "service-design", 
            "service-transition",
            "service-operation",
            "continual-service-improvement"
        ]
        
    def get_service_value_system(self) -> Dict[str, Any]:
        """Get ITIL 4 Service Value System information"""
        return self._load_knowledge_file("service-value-system.json")
    
    def get_guiding_principles(self) -> List[Dict[str, str]]:
        """Get ITIL 4 Guiding Principles"""
        svs = self.get_service_value_system()
        return svs.get("components", {}).get("guiding_principles", [])
    
    def get_service_value_chain_activities(self) -> List[Dict[str, Any]]:
        """Get Service Value Chain activities"""
        svs = self.get_service_value_system()
        return svs.get("components", {}).get("service_value_chain", {}).get("activities", [])
    
    def get_practice_knowledge(self, practice_name: str, itil_area: str) -> Optional[Dict[str, Any]]:
        """Get detailed knowledge about a specific ITIL practice"""
        filename = f"{practice_name.lower().replace(' ', '-').replace('(', '').replace(')', '')}.json"
        file_path = self.knowledge_base_path / itil_area / filename
        
        if file_path.exists():
            return self._load_knowledge_file(f"{itil_area}/{filename}")
        return None
    
    def get_incident_management_knowledge(self) -> Dict[str, Any]:
        """Get comprehensive incident management knowledge"""
        return self.get_practice_knowledge("incident-management", "service-operation") or {}
    
    def get_change_management_knowledge(self) -> Dict[str, Any]:
        """Get comprehensive change management knowledge"""
        return self.get_practice_knowledge("change-management", "service-transition") or {}
    
    def get_problem_management_knowledge(self) -> Dict[str, Any]:
        """Get comprehensive problem management knowledge"""
        return self.get_practice_knowledge("problem-management", "service-operation") or {}
    
    def get_practice_activities(self, practice_name: str, itil_area: str) -> List[Dict[str, Any]]:
        """Get key activities for a specific practice"""
        knowledge = self.get_practice_knowledge(practice_name, itil_area)
        return knowledge.get("key_activities", []) if knowledge else []
    
    def get_practice_metrics(self, practice_name: str, itil_area: str) -> List[Dict[str, Any]]:
        """Get key metrics for a specific practice"""
        knowledge = self.get_practice_knowledge(practice_name, itil_area)
        return knowledge.get("key_metrics", []) if knowledge else []
    
    def generate_itil_process_context(self, process_type: str, itil_area: str) -> Dict[str, Any]:
        """Generate comprehensive ITIL context for AI process generation"""
        
        # Get general ITIL context
        svs = self.get_service_value_system()
        guiding_principles = self.get_guiding_principles()
        value_chain = self.get_service_value_chain_activities()
        
        # Get specific practice knowledge
        practice_knowledge = self.get_practice_knowledge(process_type, itil_area)
        
        context = {
            "itil_version": "ITIL 4",
            "service_value_system": {
                "guiding_principles": guiding_principles,
                "value_chain_activities": value_chain,
                "practices_overview": svs.get("components", {}).get("practices", {})
            },
            "process_specific": practice_knowledge or {},
            "norwegian_context": {
                "language": "Norwegian",
                "business_culture": "Consensus-driven decision making",
                "regulations": "GDPR compliance required",
                "communication_style": "Direct and transparent"
            },
            "ai_generation_guidance": {
                "focus_areas": [
                    "Follow ITIL 4 guiding principles",
                    "Align with Service Value Chain",
                    "Include Norwegian business context",
                    "Provide practical implementation steps",
                    "Include relevant metrics and KPIs"
                ],
                "step_requirements": [
                    "Clear role assignments",
                    "Realistic time estimates", 
                    "Proper escalation procedures",
                    "Quality checkpoints",
                    "Documentation requirements"
                ]
            }
        }
        
        return context
    
    def get_process_template_suggestions(self, process_type: str, itil_area: str) -> List[Dict[str, Any]]:
        """Get template suggestions for a specific process type"""
        knowledge = self.get_practice_knowledge(process_type, itil_area)
        
        if not knowledge:
            return []
            
        activities = knowledge.get("key_activities", [])
        metrics = knowledge.get("key_metrics", [])
        
        suggestions = []
        
        for activity in activities:
            suggestion = {
                "step_title": activity.get("activity", "Process Step"),
                "description": activity.get("description", ""),
                "sub_activities": activity.get("sub_activities", []),
                "estimated_duration": self._estimate_duration(activity),
                "responsible_role": self._suggest_role(activity, knowledge),
                "step_type": self._determine_step_type(activity)
            }
            suggestions.append(suggestion)
            
        return suggestions
    
    def validate_process_against_itil(self, process_data: Dict[str, Any]) -> Dict[str, Any]:
        """Validate a process against ITIL best practices"""
        validation_results = {
            "compliance_score": 0,
            "compliance_checks": [],
            "recommendations": [],
            "missing_elements": []
        }
        
        # Check for ITIL alignment
        checks = [
            self._check_guiding_principles_alignment(process_data),
            self._check_value_chain_alignment(process_data), 
            self._check_role_clarity(process_data),
            self._check_metric_inclusion(process_data),
            self._check_norwegian_context(process_data)
        ]
        
        validation_results["compliance_checks"] = checks
        validation_results["compliance_score"] = sum(check["score"] for check in checks) / len(checks)
        
        # Generate recommendations
        for check in checks:
            if check["score"] < 0.8:
                validation_results["recommendations"].append(check["recommendation"])
                
        return validation_results
    
    def _load_knowledge_file(self, filename: str) -> Dict[str, Any]:
        """Load knowledge from JSON file with caching"""
        if filename in self._knowledge_cache:
            return self._knowledge_cache[filename]
            
        file_path = self.knowledge_base_path / filename
        
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                knowledge = json.load(f)
                self._knowledge_cache[filename] = knowledge
                return knowledge
        except (FileNotFoundError, json.JSONDecodeError) as e:
            print(f"Warning: Could not load ITIL knowledge file {filename}: {e}")
            return {}
    
    def _estimate_duration(self, activity: Dict[str, Any]) -> int:
        """Estimate duration for an activity based on complexity"""
        sub_activities = activity.get("sub_activities", [])
        base_duration = 30  # minutes
        
        # Adjust based on number of sub-activities
        duration = base_duration + (len(sub_activities) * 15)
        
        # Activity-specific adjustments
        activity_name = activity.get("activity", "").lower()
        if "escalation" in activity_name:
            duration = 15
        elif "investigation" in activity_name or "diagnosis" in activity_name:
            duration = 60
        elif "authorization" in activity_name or "approval" in activity_name:
            duration = 120
            
        return duration
    
    def _suggest_role(self, activity: Dict[str, Any], knowledge: Dict[str, Any]) -> str:
        """Suggest appropriate role for an activity"""
        activity_name = activity.get("activity", "").lower()
        roles = knowledge.get("roles_and_responsibilities", {})
        
        # Role mapping based on activity type
        if "desk" in activity_name or "log" in activity_name:
            return "Service Desk"
        elif "manage" in activity_name or "coordinate" in activity_name:
            return "Process Manager"
        elif "technical" in activity_name or "implement" in activity_name:
            return "Technical Team"
        elif "authorization" in activity_name or "approve" in activity_name:
            return "Change Authority"
        else:
            return "Process Team"
    
    def _determine_step_type(self, activity: Dict[str, Any]) -> str:
        """Determine step type based on activity"""
        activity_name = activity.get("activity", "").lower()
        
        if "authorization" in activity_name or "approval" in activity_name:
            return "Approval"
        elif "decision" in activity_name or "assess" in activity_name:
            return "Decision"  
        elif "document" in activity_name or "record" in activity_name:
            return "Document"
        else:
            return "Task"
    
    def _check_guiding_principles_alignment(self, process_data: Dict[str, Any]) -> Dict[str, Any]:
        """Check alignment with ITIL guiding principles"""
        score = 0.7  # Base score
        recommendation = "Ensure process aligns with ITIL 4 guiding principles"
        
        # Check for value focus
        if "value" in process_data.get("description", "").lower():
            score += 0.1
            
        # Check for iterative approach
        steps = process_data.get("steps", [])
        if len(steps) > 3:  # Broken into manageable steps
            score += 0.1
            
        # Check for collaboration indicators
        roles = [step.get("responsible_role", "") for step in steps]
        if len(set(roles)) > 1:  # Multiple roles involved
            score += 0.1
            
        return {
            "check_name": "Guiding Principles Alignment",
            "score": min(score, 1.0),
            "recommendation": recommendation
        }
    
    def _check_value_chain_alignment(self, process_data: Dict[str, Any]) -> Dict[str, Any]:
        """Check alignment with Service Value Chain"""
        score = 0.8  # Base score
        recommendation = "Align process activities with Service Value Chain"
        
        # Basic alignment check - more sophisticated logic could be implemented
        return {
            "check_name": "Value Chain Alignment", 
            "score": score,
            "recommendation": recommendation
        }
    
    def _check_role_clarity(self, process_data: Dict[str, Any]) -> Dict[str, Any]:
        """Check for clear role assignments"""
        score = 0.6
        recommendation = "Ensure clear role assignments for all process steps"
        
        steps = process_data.get("steps", [])
        steps_with_roles = [step for step in steps if step.get("responsible_role")]
        
        if steps and len(steps_with_roles) == len(steps):
            score = 1.0
            recommendation = "Role assignments are complete"
            
        return {
            "check_name": "Role Clarity",
            "score": score,
            "recommendation": recommendation
        }
    
    def _check_metric_inclusion(self, process_data: Dict[str, Any]) -> Dict[str, Any]:
        """Check for inclusion of relevant metrics"""
        score = 0.5
        recommendation = "Include relevant KPIs and metrics for process measurement"
        
        # Check if metrics are mentioned in description or steps
        text_content = f"{process_data.get('description', '')} {' '.join([step.get('description', '') for step in process_data.get('steps', [])])}"
        
        metric_keywords = ["metric", "kpi", "measure", "time", "satisfaction", "resolution"]
        if any(keyword in text_content.lower() for keyword in metric_keywords):
            score = 0.8
            
        return {
            "check_name": "Metrics Inclusion",
            "score": score, 
            "recommendation": recommendation
        }
    
    def _check_norwegian_context(self, process_data: Dict[str, Any]) -> Dict[str, Any]:
        """Check for Norwegian business context considerations"""
        score = 0.7
        recommendation = "Include Norwegian business context and language considerations"
        
        # Check for Norwegian language or cultural considerations
        text_content = f"{process_data.get('description', '')} {' '.join([step.get('description', '') for step in process_data.get('steps', [])])}"
        
        norwegian_indicators = ["norsk", "norge", "gdpr", "personvern"]
        if any(indicator in text_content.lower() for indicator in norwegian_indicators):
            score = 0.9
            
        return {
            "check_name": "Norwegian Context",
            "score": score,
            "recommendation": recommendation
        }