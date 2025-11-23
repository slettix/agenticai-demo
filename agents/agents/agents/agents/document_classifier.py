"""
AI Agent for document classification using OpenAI
Epic 3 - Story 3.1: Automatic document classification for Norwegian business documents
"""
import os
import json
from typing import Dict, Any, Callable, List
from openai import AsyncOpenAI
from datetime import datetime


class DocumentClassifierAgent:
    """AI Agent that classifies Norwegian business documents and suggests relevant processes"""
    
    def __init__(self):
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            raise ValueError("OPENAI_API_KEY environment variable must be set")
        
        self.client = AsyncOpenAI(api_key=api_key)
        self.model = os.getenv("OPENAI_MODEL", "gpt-4")
        
        # Norwegian business document types
        self.document_types = {
            "INVOICE": "Faktura",
            "CONTRACT": "Kontrakt",
            "HR_DOCUMENT": "HR-dokument",
            "POLICY": "Retningslinjer",
            "REPORT": "Rapport",
            "MEETING_MINUTES": "Møtereferat",
            "PROPOSAL": "Forslag",
            "CORRESPONDENCE": "Korrespondanse",
            "FORM": "Skjema",
            "OTHER": "Annet"
        }
        
    async def classify_document(self, request_data: Dict[str, Any], progress_callback: Callable[[int, str], None]) -> Dict[str, Any]:
        """
        Classify a document and suggest relevant processes
        """
        await progress_callback(10, "Analyzing document content...")
        
        # Extract request parameters
        document_content = request_data.get("document_content", "")
        document_type = request_data.get("document_type", "")
        context = request_data.get("context", "")
        user_id = request_data.get("user_id", "unknown")
        
        if not document_content:
            raise ValueError("Document content is required for classification")
        
        await progress_callback(20, "Preparing classification prompt...")
        
        # Build the classification prompt
        prompt = self._build_classification_prompt(document_content, document_type, context)
        
        await progress_callback(40, "Calling OpenAI API for document analysis...")
        
        try:
            # Call OpenAI API
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {
                        "role": "system",
                        "content": "Du er en ekspert på klassifisering av norske forretningsdokumenter. Du kan identifisere dokumenttyper og foreslå relevante prosesser basert på innhold."
                    },
                    {
                        "role": "user", 
                        "content": prompt
                    }
                ],
                temperature=0.3,  # Lower temperature for more consistent classification
                max_tokens=1500
            )
            
            await progress_callback(70, "Processing classification results...")
            
            # Parse the response
            ai_response = response.choices[0].message.content
            classification_data = self._parse_classification_response(ai_response)
            
            await progress_callback(90, "Finalizing classification...")
            
            # Structure the final result
            result = {
                "document_type": classification_data.get("document_type", "OTHER"),
                "document_type_norwegian": self.document_types.get(
                    classification_data.get("document_type", "OTHER"), 
                    "Annet"
                ),
                "confidence_score": classification_data.get("confidence_score", 0.5),
                "key_elements": classification_data.get("key_elements", []),
                "suggested_processes": classification_data.get("suggested_processes", []),
                "summary": classification_data.get("summary", ""),
                "business_category": classification_data.get("business_category", ""),
                "urgency_level": classification_data.get("urgency_level", "normal"),
                "metadata": {
                    "classified_at": datetime.utcnow().isoformat(),
                    "classification_model": self.model,
                    "user_id": user_id,
                    "original_type_hint": document_type,
                    "has_context": bool(context),
                    "content_length": len(document_content)
                }
            }
            
            await progress_callback(100, "Document classification completed")
            return result
            
        except Exception as e:
            raise Exception(f"Failed to classify document: {str(e)}")
    
    def _build_classification_prompt(self, content: str, suggested_type: str, context: str) -> str:
        """Build the prompt for document classification"""
        
        type_hint = f"\n**Foreslått type:** {suggested_type}" if suggested_type else ""
        context_info = f"\n**Kontekst:** {context}" if context else ""
        
        prompt = f"""
Analyser følgende norske forretningsdokument og klassifiser det:

**Dokumentinnhold:**
{content[:2000]}...{' (avkortet)' if len(content) > 2000 else ''}
{type_hint}
{context_info}

Vennligst analyser dokumentet og gi en strukturert klassifisering:

1. **Dokumenttype:** Velg fra disse kategoriene:
   - INVOICE (Faktura)
   - CONTRACT (Kontrakt) 
   - HR_DOCUMENT (HR-dokument)
   - POLICY (Retningslinjer)
   - REPORT (Rapport)
   - MEETING_MINUTES (Møtereferat)
   - PROPOSAL (Forslag)
   - CORRESPONDENCE (Korrespondanse)
   - FORM (Skjema)
   - OTHER (Annet)

2. **Nøkkelelementer:** Identifiser viktige elementer som:
   - Datoer, beløp, navn, referansenumre
   - Viktige nøkkelord og fraser
   - Strukturelle elementer

3. **Foreslåtte prosesser:** Basert på dokumenttypen, foreslå 1-3 relevante forretningsprosesser som kan være aktuelle

4. **Forretningskategori:** Kategoriser etter område (HR, Økonomi, IT, Kundeservice, etc.)

5. **Hastegrad:** Vurder om dokumentet krever rask behandling (low, normal, high, urgent)

Svar i JSON-format:
```json
{{
  "document_type": "DOCUMENT_TYPE",
  "confidence_score": 0.95,
  "summary": "Kort sammendrag av dokumentet på norsk",
  "key_elements": [
    "Nøkkelelement 1",
    "Nøkkelelement 2"
  ],
  "suggested_processes": [
    {{
      "process_name": "Prosessnavn",
      "description": "Beskrivelse av hvorfor denne prosessen er relevant",
      "priority": "high/medium/low"
    }}
  ],
  "business_category": "Forretningsområde",
  "urgency_level": "normal"
}}
```

Fokuser på nøyaktighet og gi praktiske forslag basert på norsk forretningspraksis.
"""
        return prompt.strip()
    
    def _parse_classification_response(self, response: str) -> Dict[str, Any]:
        """Parse the AI classification response"""
        try:
            # Try to find JSON in the response
            start = response.find('{')
            end = response.rfind('}') + 1
            
            if start >= 0 and end > start:
                json_str = response[start:end]
                parsed_data = json.loads(json_str)
                
                # Validate and set defaults
                result = {
                    "document_type": parsed_data.get("document_type", "OTHER"),
                    "confidence_score": min(max(parsed_data.get("confidence_score", 0.5), 0.0), 1.0),
                    "summary": parsed_data.get("summary", "Dokumentet ble klassifisert av AI"),
                    "key_elements": parsed_data.get("key_elements", []),
                    "suggested_processes": parsed_data.get("suggested_processes", []),
                    "business_category": parsed_data.get("business_category", "Generelt"),
                    "urgency_level": parsed_data.get("urgency_level", "normal")
                }
                
                # Ensure suggested_processes is properly formatted
                if isinstance(result["suggested_processes"], list):
                    formatted_processes = []
                    for process in result["suggested_processes"]:
                        if isinstance(process, dict):
                            formatted_processes.append({
                                "process_name": process.get("process_name", "Ukjent prosess"),
                                "description": process.get("description", "Ingen beskrivelse tilgjengelig"),
                                "priority": process.get("priority", "medium")
                            })
                        elif isinstance(process, str):
                            formatted_processes.append({
                                "process_name": process,
                                "description": "Foreslått basert på dokumentinnhold",
                                "priority": "medium"
                            })
                    result["suggested_processes"] = formatted_processes
                
                return result
            else:
                return self._fallback_classification()
                
        except (json.JSONDecodeError, KeyError) as e:
            # Fallback to basic classification
            return self._fallback_classification()
    
    def _fallback_classification(self) -> Dict[str, Any]:
        """Fallback classification if parsing fails"""
        return {
            "document_type": "OTHER",
            "confidence_score": 0.3,
            "summary": "Dokumentet kunne ikke klassifiseres automatisk",
            "key_elements": ["Ukjent innhold"],
            "suggested_processes": [
                {
                    "process_name": "Manuell gjennomgang",
                    "description": "Dokumentet må gjennomgås manuelt for korrekt klassifisering",
                    "priority": "medium"
                }
            ],
            "business_category": "Ukjent",
            "urgency_level": "normal"
        }