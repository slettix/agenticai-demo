"""
AI Agent for process optimization using OpenAI
Epic 3 - Story 3.2: Process optimization recommendations using AI analysis
"""
import os
import json
from typing import Dict, Any, Callable, List
from openai import AsyncOpenAI
from datetime import datetime, timedelta
import random


class ProcessOptimizerAgent:
    """AI Agent that analyzes processes and provides optimization recommendations"""
    
    def __init__(self):
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            raise ValueError("OPENAI_API_KEY environment variable must be set")
        
        self.client = AsyncOpenAI(api_key=api_key)
        self.model = os.getenv("OPENAI_MODEL", "gpt-4")
        
    async def analyze_process_performance(self, request_data: Dict[str, Any], progress_callback: Callable[[int, str], None]) -> Dict[str, Any]:
        """
        Analyze a process and provide optimization recommendations
        """
        await progress_callback(10, "Analyzing process data...")
        
        # Extract request parameters
        process_id = request_data.get("process_id")
        process_title = request_data.get("process_title", "Ukjent prosess")
        process_steps = request_data.get("process_steps", [])
        performance_metrics = request_data.get("performance_metrics", {})
        historical_data = request_data.get("historical_data", [])
        user_id = request_data.get("user_id", "unknown")
        
        if not process_id:
            raise ValueError("Process ID is required for optimization analysis")
        
        await progress_callback(20, "Generating sample metrics if needed...")
        
        # Generate sample performance data if not provided
        if not performance_metrics:
            performance_metrics = self._generate_sample_metrics(process_steps)
        
        await progress_callback(30, "Preparing optimization analysis...")
        
        # Build the optimization prompt
        prompt = self._build_optimization_prompt(
            process_title, process_steps, performance_metrics, historical_data
        )
        
        await progress_callback(50, "Calling OpenAI API for process analysis...")
        
        try:
            # Call OpenAI API
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {
                        "role": "system",
                        "content": "Du er en ekspert på prosessoptimalisering og Business Process Management (BPM). Du analyserer prosesser og gir konkrete, gjennomførbare forbedringsforslag basert på data og beste praksis."
                    },
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ],
                temperature=0.4,  # Balanced temperature for creative but consistent recommendations
                max_tokens=2000
            )
            
            await progress_callback(70, "Processing optimization recommendations...")
            
            # Parse the response
            ai_response = response.choices[0].message.content
            optimization_data = self._parse_optimization_response(ai_response)
            
            await progress_callback(85, "Calculating optimization metrics...")
            
            # Calculate additional metrics
            optimization_score = self._calculate_optimization_score(optimization_data, performance_metrics)
            
            await progress_callback(95, "Finalizing optimization report...")
            
            # Structure the final result
            result = {
                "process_id": process_id,
                "process_title": process_title,
                "analysis_date": datetime.utcnow().isoformat(),
                "optimization_score": optimization_score,
                "performance_metrics": performance_metrics,
                "bottlenecks": optimization_data.get("bottlenecks", []),
                "recommendations": optimization_data.get("recommendations", []),
                "potential_savings": optimization_data.get("potential_savings", {}),
                "implementation_complexity": optimization_data.get("implementation_complexity", "medium"),
                "priority_actions": optimization_data.get("priority_actions", []),
                "success_metrics": optimization_data.get("success_metrics", []),
                "metadata": {
                    "analyzed_at": datetime.utcnow().isoformat(),
                    "analysis_model": self.model,
                    "user_id": user_id,
                    "steps_analyzed": len(process_steps),
                    "metrics_provided": bool(request_data.get("performance_metrics")),
                    "historical_data_available": len(historical_data) > 0
                }
            }
            
            await progress_callback(100, "Process optimization analysis completed")
            return result
            
        except Exception as e:
            raise Exception(f"Failed to optimize process: {str(e)}")
    
    def _build_optimization_prompt(self, process_title: str, steps: List[Dict], metrics: Dict[str, Any], historical: List[Dict]) -> str:
        """Build the prompt for process optimization"""
        
        # Format process steps
        steps_text = "\n".join([
            f"{i+1}. {step.get('title', 'Ukjent steg')} "
            f"(Ansvarlig: {step.get('responsible_role', 'Ikke spesifisert')}, "
            f"Tid: {step.get('estimated_duration', 'Ikke spesifisert')} min)"
            for i, step in enumerate(steps)
        ]) if steps else "Ingen steg tilgjengelig"
        
        # Format metrics
        metrics_text = "\n".join([
            f"- {key}: {value}" for key, value in metrics.items()
        ]) if metrics else "Ingen ytelsesmetrikker tilgjengelig"
        
        # Format historical data
        historical_text = f"Historiske data fra {len(historical)} tidligere analyser tilgjengelig" if historical else "Ingen historiske data tilgjengelig"
        
        prompt = f"""
Analyser følgende forretningsprosess og gi konkrete optimaliseringsanbefalinger:

**Prosess:** {process_title}

**Prosesstrinn:**
{steps_text}

**Ytelsesmetrikker:**
{metrics_text}

**Historisk informasjon:**
{historical_text}

Vennligst utfør en grundig analyse og lever følgende:

1. **Flaskehalser:** Identifiser de 3 største flaskehalsene i prosessen
2. **Optimaliseringsanbefalinger:** Gi 4-6 konkrete, gjennomførbare forbedringsforslag
3. **Potensielle besparelser:** Estimer tid- og kostnadsbesparing
4. **Implementeringskompleksitet:** Vurder hvor vanskelig endringene er å implementere
5. **Prioriterte tiltak:** List de 3 viktigste tiltakene som bør gjøres først
6. **Suksessmetrikker:** Foreslå hvordan forbedringene kan måles

Fokuser på:
- Automatiseringsmuligheter
- Reduksjon av venting og unødvendige handlinger
- Forbedring av kommunikasjon og samarbeid
- Teknologiske løsninger
- Prosessstrømlinjeforming

Svar i JSON-format:
```json
{{
  "bottlenecks": [
    {{
      "step": "Stegnavn",
      "issue": "Beskrivelse av flaskehals",
      "impact": "high/medium/low",
      "current_time": 45,
      "potential_improvement": 20
    }}
  ],
  "recommendations": [
    {{
      "title": "Anbefalingtittel",
      "description": "Detaljert beskrivelse av forbedringen",
      "category": "automation/workflow/technology/training",
      "effort_required": "low/medium/high",
      "time_savings_minutes": 30,
      "cost_impact": "none/low/medium/high",
      "implementation_steps": [
        "Steg 1",
        "Steg 2"
      ]
    }}
  ],
  "potential_savings": {{
    "time_per_execution_minutes": 45,
    "annual_time_savings_hours": 120,
    "cost_savings_estimate": "Moderat",
    "efficiency_improvement_percent": 25
  }},
  "implementation_complexity": "medium",
  "priority_actions": [
    {{
      "action": "Høyprioritetstiltak",
      "rationale": "Hvorfor dette er viktig",
      "timeline": "2-4 uker"
    }}
  ],
  "success_metrics": [
    {{
      "metric": "Gjennomføringstid",
      "current_value": "120 minutter",
      "target_value": "90 minutter",
      "measurement_method": "Hvordan måle"
    }}
  ]
}}
```

Gi praktiske, norske forretningsanbefalinger basert på beste praksis innen prosessoptimalisering.
"""
        return prompt.strip()
    
    def _parse_optimization_response(self, response: str) -> Dict[str, Any]:
        """Parse the AI optimization response"""
        try:
            # Try to find JSON in the response
            start = response.find('{')
            end = response.rfind('}') + 1
            
            if start >= 0 and end > start:
                json_str = response[start:end]
                parsed_data = json.loads(json_str)
                
                # Validate and structure the data
                result = {
                    "bottlenecks": self._validate_bottlenecks(parsed_data.get("bottlenecks", [])),
                    "recommendations": self._validate_recommendations(parsed_data.get("recommendations", [])),
                    "potential_savings": self._validate_savings(parsed_data.get("potential_savings", {})),
                    "implementation_complexity": parsed_data.get("implementation_complexity", "medium"),
                    "priority_actions": self._validate_actions(parsed_data.get("priority_actions", [])),
                    "success_metrics": self._validate_metrics(parsed_data.get("success_metrics", []))
                }
                
                return result
            else:
                return self._fallback_optimization()
                
        except (json.JSONDecodeError, KeyError) as e:
            return self._fallback_optimization()
    
    def _validate_bottlenecks(self, bottlenecks: List[Dict]) -> List[Dict]:
        """Validate and format bottleneck data"""
        validated = []
        for bottleneck in bottlenecks[:5]:  # Limit to 5 bottlenecks
            if isinstance(bottleneck, dict):
                validated.append({
                    "step": bottleneck.get("step", "Ukjent steg"),
                    "issue": bottleneck.get("issue", "Ikke spesifisert"),
                    "impact": bottleneck.get("impact", "medium"),
                    "current_time": bottleneck.get("current_time", 30),
                    "potential_improvement": bottleneck.get("potential_improvement", 10)
                })
        return validated
    
    def _validate_recommendations(self, recommendations: List[Dict]) -> List[Dict]:
        """Validate and format recommendation data"""
        validated = []
        for rec in recommendations[:6]:  # Limit to 6 recommendations
            if isinstance(rec, dict):
                validated.append({
                    "title": rec.get("title", "Forbedring"),
                    "description": rec.get("description", "Ingen beskrivelse"),
                    "category": rec.get("category", "workflow"),
                    "effort_required": rec.get("effort_required", "medium"),
                    "time_savings_minutes": rec.get("time_savings_minutes", 15),
                    "cost_impact": rec.get("cost_impact", "low"),
                    "implementation_steps": rec.get("implementation_steps", ["Planlegg implementering"])
                })
        return validated
    
    def _validate_savings(self, savings: Dict) -> Dict[str, Any]:
        """Validate and format savings data"""
        return {
            "time_per_execution_minutes": savings.get("time_per_execution_minutes", 30),
            "annual_time_savings_hours": savings.get("annual_time_savings_hours", 50),
            "cost_savings_estimate": savings.get("cost_savings_estimate", "Moderat"),
            "efficiency_improvement_percent": min(max(savings.get("efficiency_improvement_percent", 15), 0), 100)
        }
    
    def _validate_actions(self, actions: List[Dict]) -> List[Dict]:
        """Validate and format priority actions"""
        validated = []
        for action in actions[:3]:  # Limit to 3 priority actions
            if isinstance(action, dict):
                validated.append({
                    "action": action.get("action", "Implementer forbedring"),
                    "rationale": action.get("rationale", "Vil gi forbedring"),
                    "timeline": action.get("timeline", "4-6 uker")
                })
        return validated
    
    def _validate_metrics(self, metrics: List[Dict]) -> List[Dict]:
        """Validate and format success metrics"""
        validated = []
        for metric in metrics[:4]:  # Limit to 4 metrics
            if isinstance(metric, dict):
                validated.append({
                    "metric": metric.get("metric", "Ytelsesmåling"),
                    "current_value": metric.get("current_value", "Ikke målt"),
                    "target_value": metric.get("target_value", "Forbedret"),
                    "measurement_method": metric.get("measurement_method", "Manuell måling")
                })
        return validated
    
    def _generate_sample_metrics(self, steps: List[Dict]) -> Dict[str, Any]:
        """Generate realistic sample performance metrics if none provided"""
        num_steps = len(steps) if steps else 5
        total_time = sum([step.get("estimated_duration", 15) for step in steps]) if steps else 75
        
        return {
            "total_execution_time_minutes": total_time + random.randint(-10, 25),
            "average_execution_time_minutes": total_time + random.randint(-5, 15),
            "success_rate_percent": random.randint(75, 95),
            "number_of_steps": num_steps,
            "manual_steps": max(1, num_steps - random.randint(0, 2)),
            "automation_level_percent": random.randint(20, 60),
            "monthly_executions": random.randint(15, 150),
            "error_rate_percent": random.randint(2, 12),
            "waiting_time_minutes": random.randint(5, 30),
            "rework_rate_percent": random.randint(5, 20)
        }
    
    def _calculate_optimization_score(self, optimization_data: Dict, metrics: Dict) -> int:
        """Calculate an overall optimization score (0-100)"""
        score = 70  # Base score
        
        # Adjust based on potential savings
        savings = optimization_data.get("potential_savings", {})
        efficiency_improvement = savings.get("efficiency_improvement_percent", 15)
        score += min(efficiency_improvement, 20)  # Max 20 points for efficiency
        
        # Adjust based on number of actionable recommendations
        recommendations = optimization_data.get("recommendations", [])
        score += min(len(recommendations) * 2, 10)  # Max 10 points for recommendations
        
        # Adjust based on current metrics
        if metrics.get("automation_level_percent", 50) < 50:
            score -= 5  # Deduct points for low automation
        
        if metrics.get("error_rate_percent", 5) > 10:
            score -= 5  # Deduct points for high error rate
        
        return min(max(score, 0), 100)  # Ensure score is between 0-100
    
    def _fallback_optimization(self) -> Dict[str, Any]:
        """Fallback optimization data if parsing fails"""
        return {
            "bottlenecks": [
                {
                    "step": "Generell flaskehals",
                    "issue": "Prosessen kunne ikke analyseres fullstendig",
                    "impact": "medium",
                    "current_time": 30,
                    "potential_improvement": 10
                }
            ],
            "recommendations": [
                {
                    "title": "Manuell prosessgjennomgang",
                    "description": "Gjennomfør en detaljert manuell analyse av prosessen",
                    "category": "workflow",
                    "effort_required": "medium",
                    "time_savings_minutes": 15,
                    "cost_impact": "low",
                    "implementation_steps": ["Planlegg gjennomgang", "Utfør analyse"]
                }
            ],
            "potential_savings": {
                "time_per_execution_minutes": 15,
                "annual_time_savings_hours": 25,
                "cost_savings_estimate": "Lav",
                "efficiency_improvement_percent": 10
            },
            "implementation_complexity": "medium",
            "priority_actions": [
                {
                    "action": "Gjennomfør manuell analyse",
                    "rationale": "Automatisk analyse var ikke fullstendig",
                    "timeline": "2-3 uker"
                }
            ],
            "success_metrics": [
                {
                    "metric": "Gjennomføringstid",
                    "current_value": "Ikke målt",
                    "target_value": "Redusert med 10%",
                    "measurement_method": "Tidsmåling"
                }
            ]
        }