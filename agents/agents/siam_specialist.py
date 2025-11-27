"""
SIAM Specialist AI Agent for Epic 7 - Issue #30
Provides expert guidance on Service Integration and Management (SIAM) for multi-vendor environments
"""
import os
import json
from typing import Dict, Any, List, Optional
from pathlib import Path
from datetime import datetime
import openai
from dotenv import load_dotenv

load_dotenv()

class SIAMSpecialistAgent:
    """AI Agent specializing in SIAM methodology and multi-vendor service integration"""
    
    def __init__(self):
        # Initialize OpenAI client
        self.client = openai.OpenAI(api_key=os.getenv("OPENAI_API_KEY"))
        
        # Path to SIAM knowledge base
        self.knowledge_base_path = Path(__file__).parent.parent.parent / "data" / "siam"
        
        # Cache for loaded knowledge
        self._knowledge_cache = {}
        
        # SIAM specialization areas
        self.specializations = [
            "multi_vendor_governance",
            "service_integration",
            "vendor_management", 
            "siam_implementation",
            "performance_management",
            "contract_structures",
            "risk_management",
            "norwegian_compliance"
        ]
        
    def get_siam_framework(self) -> Dict[str, Any]:
        """Get comprehensive SIAM framework information"""
        return self._load_knowledge_file("siam-framework.json")
    
    def get_multi_vendor_scenarios(self) -> Dict[str, Any]:
        """Get multi-vendor implementation scenarios and patterns"""
        return self._load_knowledge_file("multi-vendor-scenarios.json")
    
    def analyze_multi_vendor_scenario(self, scenario_description: str, requirements: List[str] = None) -> Dict[str, Any]:
        """Analyze a multi-vendor scenario and provide SIAM recommendations"""
        
        siam_framework = self.get_siam_framework()
        scenarios = self.get_multi_vendor_scenarios()
        
        # Create analysis prompt
        prompt = self._create_scenario_analysis_prompt(
            scenario_description, 
            requirements or [], 
            siam_framework, 
            scenarios
        )
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-4",
                messages=[
                    {
                        "role": "system",
                        "content": self._get_system_prompt()
                    },
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ],
                temperature=0.3,
                max_tokens=2000
            )
            
            analysis_content = response.choices[0].message.content
            
            return {
                "scenario_analysis": analysis_content,
                "recommendations": self._extract_recommendations(analysis_content),
                "governance_model": self._suggest_governance_model(scenario_description, siam_framework),
                "implementation_phases": self._suggest_implementation_phases(siam_framework),
                "risk_factors": self._identify_risk_factors(scenario_description),
                "success_metrics": self._suggest_success_metrics(scenario_description),
                "norwegian_considerations": self._get_norwegian_context(siam_framework),
                "timestamp": datetime.now().isoformat()
            }
            
        except Exception as e:
            return {
                "error": f"Failed to analyze scenario: {str(e)}",
                "fallback_recommendations": self._get_fallback_recommendations(),
                "timestamp": datetime.now().isoformat()
            }
    
    def provide_governance_guidance(self, vendor_count: int, service_complexity: str, org_maturity: str) -> Dict[str, Any]:
        """Provide specific governance structure recommendations"""
        
        siam_framework = self.get_siam_framework()
        scenarios = self.get_multi_vendor_scenarios()
        
        prompt = f"""
        Based on the following requirements, recommend an optimal SIAM governance structure:
        
        - Number of vendors: {vendor_count}
        - Service complexity: {service_complexity} (low/medium/high)
        - Organizational maturity: {org_maturity} (low/medium/high)
        
        Consider the SIAM framework components and governance patterns to provide:
        1. Recommended governance model (centralized/federated/hybrid)
        2. Specific governance bodies and their responsibilities
        3. Decision-making frameworks
        4. Escalation procedures
        5. Norwegian business context considerations
        
        SIAM Framework: {json.dumps(siam_framework.get('core_components', {}), indent=2)}
        Governance Patterns: {json.dumps(scenarios.get('multi_vendor_scenarios', {}).get('governance_patterns', {}), indent=2)}
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-4",
                messages=[
                    {"role": "system", "content": self._get_system_prompt()},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.2,
                max_tokens=1500
            )
            
            guidance_content = response.choices[0].message.content
            
            return {
                "governance_guidance": guidance_content,
                "recommended_model": self._determine_governance_model(vendor_count, service_complexity, org_maturity),
                "governance_bodies": self._suggest_governance_bodies(vendor_count, service_complexity),
                "implementation_timeline": self._suggest_governance_timeline(org_maturity),
                "norwegian_compliance": self._get_governance_compliance_requirements(),
                "timestamp": datetime.now().isoformat()
            }
            
        except Exception as e:
            return {
                "error": f"Failed to provide governance guidance: {str(e)}",
                "timestamp": datetime.now().isoformat()
            }
    
    def suggest_integration_approach(self, integration_requirements: Dict[str, Any]) -> Dict[str, Any]:
        """Suggest technical and process integration approaches"""
        
        siam_framework = self.get_siam_framework()
        
        prompt = f"""
        Based on the integration requirements below, provide detailed SIAM integration recommendations:
        
        Requirements: {json.dumps(integration_requirements, indent=2)}
        
        Provide recommendations for:
        1. Integration mechanisms and tools
        2. Data sharing and governance approaches
        3. Process integration strategies
        4. Performance monitoring approaches
        5. Risk mitigation strategies
        6. Implementation priorities and phasing
        
        Consider SIAM integration mechanisms: {json.dumps(siam_framework.get('core_components', {}).get('integration_mechanisms', {}), indent=2)}
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-4",
                messages=[
                    {"role": "system", "content": self._get_system_prompt()},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.3,
                max_tokens=1800
            )
            
            return {
                "integration_recommendations": response.choices[0].message.content,
                "integration_mechanisms": siam_framework.get('core_components', {}).get('integration_mechanisms', {}),
                "implementation_phases": self._suggest_integration_phases(),
                "tools_and_technologies": self._suggest_integration_tools(),
                "timestamp": datetime.now().isoformat()
            }
            
        except Exception as e:
            return {
                "error": f"Failed to suggest integration approach: {str(e)}",
                "timestamp": datetime.now().isoformat()
            }
    
    def assess_vendor_readiness(self, vendor_profiles: List[Dict[str, Any]]) -> Dict[str, Any]:
        """Assess vendor readiness for SIAM implementation"""
        
        readiness_criteria = {
            "governance_maturity": ["Established governance processes", "Clear escalation procedures", "Regular reporting capabilities"],
            "integration_capability": ["API availability", "Data sharing capabilities", "Process automation maturity"],
            "service_management": ["ITIL/ITSM processes", "Monitoring and alerting", "Incident management"],
            "cultural_alignment": ["Collaboration willingness", "Transparency practices", "Norwegian market experience"]
        }
        
        assessment_results = {}
        
        for vendor in vendor_profiles:
            vendor_name = vendor.get("name", "Unknown Vendor")
            
            prompt = f"""
            Assess this vendor's readiness for SIAM implementation:
            
            Vendor Profile: {json.dumps(vendor, indent=2)}
            
            Evaluate against these criteria:
            {json.dumps(readiness_criteria, indent=2)}
            
            Provide:
            1. Overall readiness score (1-10)
            2. Strengths and gaps by criteria
            3. Recommended preparation activities
            4. Integration complexity assessment
            5. Risk factors and mitigation strategies
            """
            
            try:
                response = self.client.chat.completions.create(
                    model="gpt-4",
                    messages=[
                        {"role": "system", "content": self._get_system_prompt()},
                        {"role": "user", "content": prompt}
                    ],
                    temperature=0.2,
                    max_tokens=1200
                )
                
                assessment_results[vendor_name] = {
                    "assessment": response.choices[0].message.content,
                    "vendor_profile": vendor
                }
                
            except Exception as e:
                assessment_results[vendor_name] = {
                    "error": f"Assessment failed: {str(e)}",
                    "vendor_profile": vendor
                }
        
        return {
            "vendor_assessments": assessment_results,
            "overall_readiness": self._calculate_overall_readiness(assessment_results),
            "critical_gaps": self._identify_critical_gaps(assessment_results),
            "readiness_criteria": readiness_criteria,
            "timestamp": datetime.now().isoformat()
        }
    
    def generate_sla_framework(self, service_requirements: Dict[str, Any]) -> Dict[str, Any]:
        """Generate SLA framework for multi-vendor environment"""
        
        prompt = f"""
        Generate a comprehensive SLA framework for a multi-vendor SIAM environment:
        
        Service Requirements: {json.dumps(service_requirements, indent=2)}
        
        Provide:
        1. End-to-end service level definitions
        2. Vendor-specific SLA components
        3. Integration and handoff SLAs
        4. Measurement and reporting framework
        5. Penalty and incentive structures
        6. Governance and review processes
        7. Norwegian legal and compliance considerations
        
        Consider SIAM-specific challenges like cross-vendor dependencies and shared accountability.
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-4",
                messages=[
                    {"role": "system", "content": self._get_system_prompt()},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.3,
                max_tokens=2000
            )
            
            return {
                "sla_framework": response.choices[0].message.content,
                "key_metrics": self._define_siam_metrics(),
                "measurement_approach": self._suggest_measurement_approach(),
                "governance_integration": self._suggest_sla_governance(),
                "norwegian_considerations": self._get_sla_legal_considerations(),
                "timestamp": datetime.now().isoformat()
            }
            
        except Exception as e:
            return {
                "error": f"Failed to generate SLA framework: {str(e)}",
                "timestamp": datetime.now().isoformat()
            }
    
    # Private helper methods
    
    def _get_system_prompt(self) -> str:
        """Get the system prompt for the SIAM specialist"""
        return """You are an expert SIAM (Service Integration and Management) consultant with deep expertise in:

- Multi-vendor service integration and governance
- SIAM framework implementation and best practices
- Norwegian business context and regulatory requirements
- Vendor management and performance optimization
- Risk management in complex service environments
- Contract structuring for multi-vendor arrangements

Provide practical, actionable advice based on proven SIAM methodologies. Consider Norwegian business culture, 
regulatory requirements (GDPR, local compliance), and collaborative decision-making preferences.

Always structure your responses clearly with specific recommendations, implementation steps, and success criteria.
Focus on value delivery and practical implementation rather than theoretical concepts."""
    
    def _create_scenario_analysis_prompt(self, scenario: str, requirements: List[str], framework: Dict, scenarios: Dict) -> str:
        """Create a comprehensive scenario analysis prompt"""
        return f"""
        Analyze the following multi-vendor scenario and provide detailed SIAM recommendations:
        
        SCENARIO: {scenario}
        
        SPECIFIC REQUIREMENTS:
        {chr(10).join(f"- {req}" for req in requirements)}
        
        Based on the SIAM framework and reference scenarios, provide:
        1. Scenario classification and complexity assessment
        2. Recommended SIAM approach and governance model
        3. Integration requirements and mechanisms
        4. Risk factors and mitigation strategies
        5. Implementation roadmap with phases
        6. Success metrics and KPIs
        7. Norwegian context considerations
        
        Consider these SIAM implementation patterns:
        {json.dumps(scenarios.get('multi_vendor_scenarios', {}).get('scenarios', []), indent=2)[:1000]}...
        
        Governance options:
        {json.dumps(scenarios.get('multi_vendor_scenarios', {}).get('governance_patterns', {}), indent=2)}
        """
    
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
            print(f"Warning: Could not load SIAM knowledge file {filename}: {e}")
            return {}
    
    def _extract_recommendations(self, analysis_content: str) -> List[str]:
        """Extract key recommendations from analysis content"""
        # Simple extraction logic - could be enhanced with NLP
        recommendations = []
        lines = analysis_content.split('\n')
        
        for line in lines:
            if line.strip().startswith('- ') or line.strip().startswith('1.') or line.strip().startswith('2.'):
                recommendations.append(line.strip())
        
        return recommendations[:10]  # Top 10 recommendations
    
    def _suggest_governance_model(self, scenario: str, framework: Dict) -> str:
        """Suggest appropriate governance model based on scenario"""
        scenario_lower = scenario.lower()
        
        if "complex" in scenario_lower or "critical" in scenario_lower or "many vendor" in scenario_lower:
            return "Centralized governance with strong SIAM authority"
        elif "specialized" in scenario_lower or "domain" in scenario_lower:
            return "Federated governance with domain expertise"
        else:
            return "Hybrid governance balancing control and flexibility"
    
    def _suggest_implementation_phases(self, framework: Dict) -> List[Dict[str, Any]]:
        """Get implementation phases from framework"""
        return framework.get('implementation_phases', [])
    
    def _identify_risk_factors(self, scenario: str) -> List[str]:
        """Identify common risk factors based on scenario"""
        risks = []
        scenario_lower = scenario.lower()
        
        if "multiple" in scenario_lower or "many" in scenario_lower:
            risks.append("Coordination complexity between multiple vendors")
        if "critical" in scenario_lower or "important" in scenario_lower:
            risks.append("Service availability and business continuity risks")
        if "integration" in scenario_lower:
            risks.append("Technical integration complexity and dependencies")
        if "new" in scenario_lower or "transform" in scenario_lower:
            risks.append("Change management and adoption challenges")
        
        risks.extend([
            "Vendor performance variability",
            "Governance and accountability gaps", 
            "Cultural and communication challenges"
        ])
        
        return risks[:7]  # Top risk factors
    
    def _suggest_success_metrics(self, scenario: str) -> List[str]:
        """Suggest success metrics based on scenario"""
        return [
            "End-to-end service availability and performance",
            "Vendor coordination and integration effectiveness",
            "Cost optimization and value delivery",
            "Customer satisfaction and user experience",
            "Incident resolution time across vendors",
            "Governance effectiveness and decision-making speed",
            "Compliance and risk management effectiveness"
        ]
    
    def _get_norwegian_context(self, framework: Dict) -> Dict[str, Any]:
        """Get Norwegian-specific considerations"""
        return framework.get('norwegian_context', {})
    
    def _get_fallback_recommendations(self) -> List[str]:
        """Provide fallback recommendations when AI analysis fails"""
        return [
            "Establish clear SIAM governance structure with defined roles",
            "Implement comprehensive vendor management processes",
            "Create integrated service catalog and SLA framework",
            "Establish end-to-end performance monitoring",
            "Implement risk management and escalation procedures",
            "Ensure compliance with Norwegian regulatory requirements"
        ]
    
    def _determine_governance_model(self, vendor_count: int, complexity: str, maturity: str) -> str:
        """Determine optimal governance model"""
        if vendor_count >= 5 or complexity == "high":
            return "Centralized"
        elif vendor_count <= 2 and complexity == "low" and maturity == "high":
            return "Federated"
        else:
            return "Hybrid"
    
    def _suggest_governance_bodies(self, vendor_count: int, complexity: str) -> List[Dict[str, str]]:
        """Suggest appropriate governance bodies"""
        bodies = [
            {"name": "SIAM Board", "purpose": "Strategic oversight and major decisions"},
            {"name": "Service Integration Team", "purpose": "Operational coordination and integration"}
        ]
        
        if vendor_count >= 3:
            bodies.append({"name": "Vendor Council", "purpose": "Multi-vendor collaboration and issue resolution"})
        
        if complexity in ["medium", "high"]:
            bodies.append({"name": "Technical Working Groups", "purpose": "Domain-specific technical coordination"})
        
        return bodies
    
    def _suggest_governance_timeline(self, maturity: str) -> Dict[str, str]:
        """Suggest governance implementation timeline"""
        if maturity == "low":
            return {"phase1": "3-6 months", "phase2": "6-12 months", "optimization": "12+ months"}
        elif maturity == "medium":
            return {"phase1": "2-4 months", "phase2": "4-8 months", "optimization": "8+ months"}
        else:
            return {"phase1": "1-3 months", "phase2": "3-6 months", "optimization": "6+ months"}
    
    def _get_governance_compliance_requirements(self) -> List[str]:
        """Get Norwegian compliance requirements for governance"""
        return [
            "GDPR compliance for data sharing between vendors",
            "Norwegian Data Protection Act adherence",
            "Public procurement regulation compliance (if applicable)",
            "Financial services regulation compliance (if applicable)",
            "Transparent decision-making documentation"
        ]
    
    def _suggest_integration_phases(self) -> List[Dict[str, str]]:
        """Suggest integration implementation phases"""
        return [
            {"phase": "Planning and Design", "duration": "2-3 months", "focus": "Integration architecture and tool selection"},
            {"phase": "Pilot Implementation", "duration": "1-2 months", "focus": "Limited scope integration testing"},
            {"phase": "Phased Rollout", "duration": "3-6 months", "focus": "Progressive integration of all services"},
            {"phase": "Optimization", "duration": "Ongoing", "focus": "Continuous improvement and automation"}
        ]
    
    def _suggest_integration_tools(self) -> List[Dict[str, str]]:
        """Suggest integration tools and technologies"""
        return [
            {"category": "Service Management", "tools": "ServiceNow, Remedy, JIRA Service Management"},
            {"category": "Monitoring", "tools": "Splunk, Dynatrace, New Relic, Grafana"},
            {"category": "Automation", "tools": "Ansible, Puppet, Terraform"},
            {"category": "Collaboration", "tools": "Microsoft Teams, Slack, Confluence"},
            {"category": "Data Integration", "tools": "MuleSoft, Informatica, Apache Kafka"}
        ]
    
    def _calculate_overall_readiness(self, assessments: Dict) -> str:
        """Calculate overall vendor readiness"""
        if not assessments:
            return "Unable to assess"
        
        # Simple heuristic - in real implementation, would parse AI responses for scores
        total_vendors = len(assessments)
        vendors_with_errors = len([v for v in assessments.values() if "error" in v])
        
        if vendors_with_errors > total_vendors * 0.5:
            return "Low readiness - multiple assessment failures"
        elif total_vendors >= 4:
            return "Medium readiness - complex multi-vendor environment"
        else:
            return "High readiness - manageable vendor count"
    
    def _identify_critical_gaps(self, assessments: Dict) -> List[str]:
        """Identify critical gaps across vendor assessments"""
        return [
            "Integration capability standardization needed",
            "Governance process alignment required", 
            "Performance monitoring consistency gaps",
            "Cultural alignment and collaboration readiness"
        ]
    
    def _define_siam_metrics(self) -> List[Dict[str, str]]:
        """Define key SIAM performance metrics"""
        return [
            {"metric": "End-to-End Service Availability", "target": "99.9%", "measurement": "Integrated monitoring across all vendors"},
            {"metric": "Cross-Vendor Incident Resolution", "target": "<4 hours", "measurement": "Time from incident to resolution across vendor boundaries"},
            {"metric": "Service Integration Effectiveness", "target": ">90%", "measurement": "Successful handoffs and integrations"},
            {"metric": "Vendor Coordination Score", "target": ">4.5/5", "measurement": "Regular vendor collaboration assessment"},
            {"metric": "Customer Satisfaction", "target": ">4.2/5", "measurement": "End-user satisfaction with integrated services"}
        ]
    
    def _suggest_measurement_approach(self) -> Dict[str, str]:
        """Suggest measurement approach for SIAM metrics"""
        return {
            "frequency": "Real-time monitoring with weekly/monthly reporting",
            "tools": "Integrated dashboards combining data from all vendors",
            "governance": "Monthly performance reviews with all stakeholders",
            "improvement": "Quarterly optimization initiatives based on metrics"
        }
    
    def _suggest_sla_governance(self) -> List[str]:
        """Suggest SLA governance approach"""
        return [
            "Monthly SLA performance review meetings with all vendors",
            "Quarterly SLA framework review and optimization",
            "Annual SLA contract and target updates",
            "Real-time performance dashboards for transparency",
            "Escalation procedures for SLA breaches"
        ]
    
    def _get_sla_legal_considerations(self) -> List[str]:
        """Get Norwegian legal considerations for SLAs"""
        return [
            "Ensure SLA terms comply with Norwegian contract law",
            "Include GDPR compliance requirements in SLAs",
            "Address liability and responsibility in multi-vendor context",
            "Include Norwegian language requirements where appropriate",
            "Consider public sector procurement requirements if applicable"
        ]