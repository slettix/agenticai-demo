import React, { useState, useEffect } from 'react';
import { ProcessStep } from './ProcessStepBuilder';
import './AIProcessSuggestions.css';

interface AISuggestion {
  id: string;
  type: 'step' | 'improvement' | 'template';
  title: string;
  description: string;
  confidence: number;
  itilAlignment?: string;
  suggestedStep?: ProcessStep;
  reasoning: string;
  category: 'efficiency' | 'compliance' | 'quality' | 'automation';
}

interface AIProcessSuggestionsProps {
  processTitle: string;
  processDescription: string;
  category: string;
  itilArea?: string;
  currentSteps: ProcessStep[];
  onApplySuggestion: (suggestion: AISuggestion) => void;
  onGenerateSteps: () => void;
  isLoading?: boolean;
}

export const AIProcessSuggestions: React.FC<AIProcessSuggestionsProps> = ({
  processTitle,
  processDescription,
  category,
  itilArea,
  currentSteps,
  onApplySuggestion,
  onGenerateSteps,
  isLoading = false
}) => {
  const [suggestions, setSuggestions] = useState<AISuggestion[]>([]);
  const [activeTab, setActiveTab] = useState<'suggestions' | 'generate' | 'insights'>('suggestions');
  const [generatingSuggestions, setGeneratingSuggestions] = useState(false);

  // Mock AI suggestions based on process context
  useEffect(() => {
    const generateMockSuggestions = () => {
      setGeneratingSuggestions(true);
      
      // Simulate AI processing delay
      setTimeout(() => {
        const mockSuggestions = generateContextualSuggestions();
        setSuggestions(mockSuggestions);
        setGeneratingSuggestions(false);
      }, 1500);
    };

    if (processTitle || processDescription) {
      generateMockSuggestions();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [processTitle, processDescription, category, itilArea]);


  const generateContextualSuggestions = (): AISuggestion[] => {
    const suggestions: AISuggestion[] = [];

    // Step suggestions based on ITIL area
    if (itilArea === 'Service Operation') {
      suggestions.push({
        id: 'step_1',
        type: 'step',
        title: 'Legg til innledende diagnostikk',
        description: 'Utfør grunnleggende diagnostikk før eskalering',
        confidence: 0.92,
        itilAlignment: 'Incident Management',
        suggestedStep: {
          id: 'ai_step_1',
          title: 'Utfør innledende diagnostikk',
          description: 'Samle informasjon og utfør grunnleggende feilsøking før videre eskalering',
          type: 'Task',
          responsibleRole: 'Service Desk',
          estimatedDuration: 15,
          orderIndex: 2,
          isOptional: false,
          detailedInstructions: 'Sjekk kjente feil, systemstatus og grunnleggende feilsøking',
          itilGuidance: 'ITIL anbefaler grundig innledende diagnostikk for å redusere eskaleringer'
        },
        reasoning: 'ITIL Incident Management anbefaler innledende diagnostikk for å redusere unødvendige eskaleringer og forbedre førstegangsløsning.',
        category: 'efficiency'
      });
    }

    // Improvement suggestions based on current steps
    if (currentSteps.length > 0) {
      const missingApproval = !currentSteps.some(step => step.type === 'Approval');
      if (missingApproval && category.toLowerCase().includes('endring')) {
        suggestions.push({
          id: 'improvement_1',
          type: 'improvement',
          title: 'Legg til godkjenningstrinn',
          description: 'Prosessen mangler formelle godkjenningspunkter',
          confidence: 0.88,
          itilAlignment: 'Change Management',
          reasoning: 'Endringsprosesser krever formell godkjenning i henhold til ITIL beste praksis.',
          category: 'compliance'
        });
      }

      // Quality suggestions
      const shortSteps = currentSteps.filter(step => step.estimatedDuration < 10);
      if (shortSteps.length > 3) {
        suggestions.push({
          id: 'improvement_2',
          type: 'improvement',
          title: 'Kombiner korte trinn',
          description: `${shortSteps.length} trinn har svært kort varighet. Vurder å kombinere relaterte oppgaver.`,
          confidence: 0.76,
          reasoning: 'Mange korte trinn kan skape unødvendige overganger. Kombinering kan forbedre flyt og effektivitet.',
          category: 'efficiency'
        });
      }
    }

    // Template suggestions
    if (itilArea) {
      suggestions.push({
        id: 'template_1',
        type: 'template',
        title: `Bruk ${itilArea} beste praksis`,
        description: `Implementer standardiserte trinn fra ITIL ${itilArea}`,
        confidence: 0.94,
        itilAlignment: itilArea,
        reasoning: `ITIL ${itilArea} gir bevist rammeverk for denne typen prosess med fokus på kvalitet og konsistens.`,
        category: 'compliance'
      });
    }

    // Automation suggestions
    if (processTitle.toLowerCase().includes('registr') || processTitle.toLowerCase().includes('log')) {
      suggestions.push({
        id: 'improvement_3',
        type: 'improvement',
        title: 'Automatiseringsmulighet',
        description: 'Deler av registreringsprosessen kan automatiseres',
        confidence: 0.82,
        reasoning: 'Automatisering av registrering kan redusere manuell innsats og forbedre datakvalitet.',
        category: 'automation'
      });
    }

    return suggestions.sort((a, b) => b.confidence - a.confidence);
  };

  const getSuggestionIcon = (type: AISuggestion['type']) => {
    switch (type) {
      case 'step': return '➕';
      case 'improvement': return '🔧';
      case 'template': return '📋';
      default: return '💡';
    }
  };

  const getCategoryColor = (category: AISuggestion['category']) => {
    switch (category) {
      case 'efficiency': return '#28a745';
      case 'compliance': return '#007bff';
      case 'quality': return '#6610f2';
      case 'automation': return '#fd7e14';
      default: return '#6c757d';
    }
  };

  const getCategoryLabel = (category: AISuggestion['category']) => {
    switch (category) {
      case 'efficiency': return 'Effektivitet';
      case 'compliance': return 'Samsvar';
      case 'quality': return 'Kvalitet';
      case 'automation': return 'Automatisering';
      default: return 'Generell';
    }
  };

  if (!processTitle && !processDescription) {
    return (
      <div className="ai-suggestions empty">
        <div className="empty-state">
          <h3>🤖 AI-drevne prosessforslag</h3>
          <p>Fyll ut prosessinformasjon for å få intelligente forslag og anbefalinger.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="ai-suggestions">
      <div className="suggestions-header">
        <h3>🤖 AI-drevne prosessforslag</h3>
        <div className="tab-navigation">
          <button 
            className={`tab ${activeTab === 'suggestions' ? 'active' : ''}`}
            onClick={() => setActiveTab('suggestions')}
          >
            💡 Forslag ({suggestions.length})
          </button>
          <button 
            className={`tab ${activeTab === 'generate' ? 'active' : ''}`}
            onClick={() => setActiveTab('generate')}
          >
            🎯 Generer trinn
          </button>
          <button 
            className={`tab ${activeTab === 'insights' ? 'active' : ''}`}
            onClick={() => setActiveTab('insights')}
          >
            📊 Innsikt
          </button>
        </div>
      </div>

      {activeTab === 'suggestions' && (
        <div className="suggestions-content">
          {generatingSuggestions ? (
            <div className="generating-state">
              <div className="spinner"></div>
              <p>AI analyserer prosessen og genererer forslag...</p>
            </div>
          ) : suggestions.length > 0 ? (
            <div className="suggestions-list">
              {suggestions.map(suggestion => (
                <div key={suggestion.id} className="suggestion-card">
                  <div className="suggestion-header">
                    <span className="suggestion-icon">
                      {getSuggestionIcon(suggestion.type)}
                    </span>
                    <h4>{suggestion.title}</h4>
                    <div className="suggestion-meta">
                      <span 
                        className="category-badge"
                        style={{ backgroundColor: getCategoryColor(suggestion.category) }}
                      >
                        {getCategoryLabel(suggestion.category)}
                      </span>
                      <span className="confidence-score">
                        {Math.round(suggestion.confidence * 100)}% tillit
                      </span>
                    </div>
                  </div>

                  <div className="suggestion-content">
                    <p className="description">{suggestion.description}</p>
                    
                    {suggestion.itilAlignment && (
                      <div className="itil-alignment">
                        <span>🎯 ITIL: {suggestion.itilAlignment}</span>
                      </div>
                    )}

                    <div className="reasoning">
                      <strong>Begrunnelse:</strong> {suggestion.reasoning}
                    </div>
                  </div>

                  <div className="suggestion-actions">
                    <button 
                      className="apply-btn"
                      onClick={() => onApplySuggestion(suggestion)}
                      disabled={isLoading}
                    >
                      {suggestion.type === 'step' ? 'Legg til trinn' : 'Anvend forslag'}
                    </button>
                    <button className="learn-more-btn">
                      Lær mer
                    </button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="no-suggestions">
              <p>Ingen forslag tilgjengelig for øyeblikket. AI vil analysere prosessen når mer informasjon er tilgjengelig.</p>
            </div>
          )}
        </div>
      )}

      {activeTab === 'generate' && (
        <div className="generate-content">
          <div className="generate-header">
            <h4>🎯 AI-generert prosesstrinn</h4>
            <p>La AI generere komplette prosesstrinn basert på din beskrivelse og ITIL beste praksis.</p>
          </div>

          <div className="generation-preview">
            <h5>AI vil generere trinn basert på:</h5>
            <ul>
              <li><strong>Prosess:</strong> {processTitle || 'Ikke spesifisert'}</li>
              <li><strong>Beskrivelse:</strong> {processDescription ? processDescription.substring(0, 100) + '...' : 'Ikke spesifisert'}</li>
              <li><strong>Kategori:</strong> {category || 'Ikke spesifisert'}</li>
              {itilArea && <li><strong>ITIL-område:</strong> {itilArea}</li>}
              <li><strong>Estimert antall trinn:</strong> 5-12 trinn</li>
              <li><strong>ITIL-samsvar:</strong> Automatisk validering</li>
            </ul>
          </div>

          <div className="generate-actions">
            <button 
              className="generate-btn primary"
              onClick={onGenerateSteps}
              disabled={isLoading || !processTitle || !processDescription}
            >
              {isLoading ? 'Genererer...' : '🚀 Generer prosesstrinn med AI'}
            </button>
            
            {(!processTitle || !processDescription) && (
              <p className="requirement-note">
                * Prosesstittel og beskrivelse må være fylt ut for å generere trinn
              </p>
            )}
          </div>
        </div>
      )}

      {activeTab === 'insights' && (
        <div className="insights-content">
          <h4>📊 Prosessinnsikt</h4>
          
          <div className="insights-grid">
            <div className="insight-card">
              <h5>🎯 Prosessmodenhet</h5>
              <div className="maturity-score">
                <span className="score">75%</span>
                <span className="label">Moderat modenhet</span>
              </div>
              <p>Prosessen følger grunnleggende ITIL-prinsipper, men har rom for forbedring.</p>
            </div>

            <div className="insight-card">
              <h5>⚡ Effektivitetspotensial</h5>
              <div className="efficiency-metrics">
                <div className="metric">
                  <span className="value">25%</span>
                  <span className="label">Tidsbesparelse</span>
                </div>
                <div className="metric">
                  <span className="value">3</span>
                  <span className="label">Automatiserbare trinn</span>
                </div>
              </div>
            </div>

            <div className="insight-card">
              <h5>🔧 Forbedringsmuligheter</h5>
              <ul>
                <li>Standardiser roller og ansvar</li>
                <li>Implementer kvalitetssjekkpunkter</li>
                <li>Legg til måling og KPI-er</li>
                <li>Vurder automatisering av rutineoppgaver</li>
              </ul>
            </div>

            <div className="insight-card">
              <h5>📈 ITIL-samsvar</h5>
              <div className="compliance-breakdown">
                <div className="compliance-item">
                  <span>Rollen definerer</span>
                  <span className="score good">85%</span>
                </div>
                <div className="compliance-item">
                  <span>Prosessflyt</span>
                  <span className="score moderate">70%</span>
                </div>
                <div className="compliance-item">
                  <span>Måling & KPI</span>
                  <span className="score poor">45%</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};