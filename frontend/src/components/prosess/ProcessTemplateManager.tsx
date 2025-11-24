import React, { useState, useEffect } from 'react';
import { ProcessStep } from './ProcessStepBuilder';
import './ProcessTemplateManager.css';

interface ProcessTemplate {
  id: string;
  name: string;
  description: string;
  itilArea: string;
  category: string;
  purpose: string;
  keyActivities: string[];
  inputs: string[];
  outputs: string[];
  kpis: string[];
  roles: string[];
  defaultSteps: ProcessStep[];
  complianceRequirements: ComplianceRequirement[];
  estimatedDuration: number;
  maturityLevel: 'Basic' | 'Intermediate' | 'Advanced';
  norwegianContext: boolean;
}

interface ComplianceRequirement {
  id: string;
  type: 'ITIL' | 'ISO' | 'GDPR' | 'Norwegian';
  requirement: string;
  description: string;
  mandatory: boolean;
  validationRules: string[];
}

interface ProcessTemplateManagerProps {
  itilArea?: string;
  category?: string;
  onTemplateSelect: (template: ProcessTemplate) => void;
  onValidateCompliance: (process: any) => ComplianceResult;
  selectedTemplate?: ProcessTemplate | null;
}

interface ComplianceResult {
  score: number;
  status: 'compliant' | 'partial' | 'non-compliant';
  passedChecks: ComplianceCheck[];
  failedChecks: ComplianceCheck[];
  recommendations: string[];
}

interface ComplianceCheck {
  id: string;
  type: string;
  description: string;
  status: 'pass' | 'fail' | 'warning';
  details?: string;
}

export const ProcessTemplateManager: React.FC<ProcessTemplateManagerProps> = ({
  itilArea,
  category,
  onTemplateSelect,
  onValidateCompliance,
  selectedTemplate
}) => {
  const [templates, setTemplates] = useState<ProcessTemplate[]>([]);
  const [filteredTemplates, setFilteredTemplates] = useState<ProcessTemplate[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterITIL, setFilterITIL] = useState(itilArea || '');
  const [filterMaturity, setFilterMaturity] = useState('');
  const [activeTab, setActiveTab] = useState<'browse' | 'compliance' | 'custom'>('browse');
  const [loadingTemplates, setLoadingTemplates] = useState(true);

  // Mock templates data
  useEffect(() => {
    loadTemplates();
  }, []);

  // Filter templates based on criteria
  useEffect(() => {
    let filtered = templates;

    if (searchTerm) {
      filtered = filtered.filter(template =>
        template.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        template.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        template.keyActivities.some(activity => 
          activity.toLowerCase().includes(searchTerm.toLowerCase())
        )
      );
    }

    if (filterITIL) {
      filtered = filtered.filter(template => template.itilArea === filterITIL);
    }

    if (filterMaturity) {
      filtered = filtered.filter(template => template.maturityLevel === filterMaturity);
    }

    setFilteredTemplates(filtered);
  }, [templates, searchTerm, filterITIL, filterMaturity]);

  const loadTemplates = () => {
    setLoadingTemplates(true);
    
    // Simulate API call
    setTimeout(() => {
      const mockTemplates: ProcessTemplate[] = [
        {
          id: 'incident-basic',
          name: 'Incident Management - Grunnleggende',
          description: 'Standard incident management prosess for norske organisasjoner',
          itilArea: 'Service Operation',
          category: 'ITSM',
          purpose: 'Gjenopprette normal tjenesteproduksjon så raskt som mulig',
          keyActivities: [
            'Incident logging og kategorisering',
            'Initial diagnostikk',
            'Eskalering og koordinering',
            'Løsning og gjenoppretting',
            'Avslutning og oppfølging'
          ],
          inputs: ['Brukerrapporter', 'Monitoring alerts', 'Service Desk henvendelser'],
          outputs: ['Løste incidents', 'Incident dokumentasjon', 'Problem records'],
          kpis: ['MTTR', 'First Call Resolution', 'Customer Satisfaction'],
          roles: ['Service Desk', 'Incident Manager', 'Technical Teams', 'Suppliers'],
          defaultSteps: [
            {
              id: 'step_1',
              title: 'Registrer incident',
              description: 'Log incident med alle relevante detaljer',
              type: 'Task',
              responsibleRole: 'Service Desk',
              estimatedDuration: 5,
              orderIndex: 1,
              isOptional: false,
              detailedInstructions: 'Registrer incident i ITSM-verktøy med kategori, prioritet og beskrivelse',
              itilGuidance: 'Nøyaktig logging er kritisk for sporbarhet og rapportering'
            },
            {
              id: 'step_2',
              title: 'Kategoriser og prioriter',
              description: 'Bestem impact, urgency og priority',
              type: 'Decision',
              responsibleRole: 'Service Desk',
              estimatedDuration: 3,
              orderIndex: 2,
              isOptional: false,
              detailedInstructions: 'Bruk prioritetsmatrise basert på impact og urgency',
              itilGuidance: 'Korrekt prioritering sikrer riktig ressursallokering'
            },
            {
              id: 'step_3',
              title: 'Utfør innledende diagnostikk',
              description: 'Forsøk å løse incident på første nivå',
              type: 'Task',
              responsibleRole: 'Service Desk',
              estimatedDuration: 15,
              orderIndex: 3,
              isOptional: false,
              detailedInstructions: 'Sjekk kjente feil og utfør standard feilsøking',
              itilGuidance: 'God diagnostikk reduserer eskaleringsbehovet'
            },
            {
              id: 'step_4',
              title: 'Eskaler hvis nødvendig',
              description: 'Videresend til spesialister hvis ikke løst',
              type: 'Gateway',
              responsibleRole: 'Service Desk',
              estimatedDuration: 2,
              orderIndex: 4,
              isOptional: true,
              detailedInstructions: 'Eskaler til relevant teknisk team med all informasjon',
              itilGuidance: 'Effektiv eskalering krever god kunnskapsoverføring'
            },
            {
              id: 'step_5',
              title: 'Løs incident',
              description: 'Implementer løsning eller workaround',
              type: 'Task',
              responsibleRole: 'Technical Team',
              estimatedDuration: 30,
              orderIndex: 5,
              isOptional: false,
              detailedInstructions: 'Implementer løsning og dokumenter framgangsmåte',
              itilGuidance: 'Løsning skal være dokumentert for fremtidig bruk'
            },
            {
              id: 'step_6',
              title: 'Verifiser løsning',
              description: 'Bekreft at tjenesten er gjenopprettet',
              type: 'Task',
              responsibleRole: 'Service Desk',
              estimatedDuration: 5,
              orderIndex: 6,
              isOptional: false,
              detailedInstructions: 'Test tjeneste og bekreft med bruker',
              itilGuidance: 'Verifikasjon sikrer at incidenten faktisk er løst'
            },
            {
              id: 'step_7',
              title: 'Avslutt incident',
              description: 'Lukk incident og oppdater dokumentasjon',
              type: 'Document',
              responsibleRole: 'Service Desk',
              estimatedDuration: 3,
              orderIndex: 7,
              isOptional: false,
              detailedInstructions: 'Oppdater incident record og lukk sak',
              itilGuidance: 'Fullstendig dokumentasjon er nødvendig for læring'
            }
          ],
          complianceRequirements: [
            {
              id: 'itil_logging',
              type: 'ITIL',
              requirement: 'Alle incidents må logges',
              description: 'ITIL krever fullstendig logging av alle incidents',
              mandatory: true,
              validationRules: ['Has incident logging step', 'Assigns responsible role']
            },
            {
              id: 'gdpr_data',
              type: 'GDPR',
              requirement: 'Persondata må håndteres i henhold til GDPR',
              description: 'Incident data kan inneholde personopplysninger',
              mandatory: true,
              validationRules: ['Data protection measures', 'Access controls']
            }
          ],
          estimatedDuration: 63,
          maturityLevel: 'Basic',
          norwegianContext: true
        },
        {
          id: 'change-advanced',
          name: 'Change Management - Avansert',
          description: 'Omfattende change management for kritiske endringer',
          itilArea: 'Service Transition',
          category: 'ITSM',
          purpose: 'Håndtere komplekse endringer med minimal risiko',
          keyActivities: [
            'Change request vurdering',
            'Risk assessment og planlegging',
            'Change Advisory Board (CAB)',
            'Implementering og overvåking',
            'Post implementation review'
          ],
          inputs: ['Change requests', 'Impact assessments', 'Risk registers'],
          outputs: ['Approved changes', 'Implementation plans', 'Change calendar'],
          kpis: ['Change success rate', 'Emergency changes %', 'Lead time'],
          roles: ['Change Manager', 'CAB', 'Change Authority', 'Implementers'],
          defaultSteps: [
            {
              id: 'change_1',
              title: 'Registrer change request',
              description: 'Formell registrering av endringsforespørsel',
              type: 'Document',
              responsibleRole: 'Change Manager',
              estimatedDuration: 10,
              orderIndex: 1,
              isOptional: false,
              detailedInstructions: 'Registrer RFC med full begrunnelse og impact analysis',
              itilGuidance: 'RFC må inneholde alle påkrevde elementer'
            }
          ],
          complianceRequirements: [
            {
              id: 'change_authorization',
              type: 'ITIL',
              requirement: 'Alle changes må være autorisert',
              description: 'ITIL krever formell autorisering før implementering',
              mandatory: true,
              validationRules: ['Has authorization step', 'Defines approval authority']
            }
          ],
          estimatedDuration: 240,
          maturityLevel: 'Advanced',
          norwegianContext: true
        }
      ];

      setTemplates(mockTemplates);
      setLoadingTemplates(false);
    }, 1000);
  };

  const getMaturityColor = (level: string) => {
    switch (level) {
      case 'Basic': return '#28a745';
      case 'Intermediate': return '#ffc107';
      case 'Advanced': return '#dc3545';
      default: return '#6c757d';
    }
  };

  const validateProcessCompliance = (template: ProcessTemplate): ComplianceResult => {
    const passedChecks: ComplianceCheck[] = [];
    const failedChecks: ComplianceCheck[] = [];
    const recommendations: string[] = [];

    // Mock compliance validation
    template.complianceRequirements.forEach(req => {
      const checkPassed = Math.random() > 0.3; // Mock validation
      const check: ComplianceCheck = {
        id: req.id,
        type: req.type,
        description: req.requirement,
        status: checkPassed ? 'pass' : 'fail',
        details: checkPassed ? 'Krav oppfylt' : 'Krav ikke oppfylt'
      };

      if (checkPassed) {
        passedChecks.push(check);
      } else {
        failedChecks.push(check);
        recommendations.push(`Implementer ${req.requirement.toLowerCase()}`);
      }
    });

    const score = passedChecks.length / (passedChecks.length + failedChecks.length) * 100;
    const status = score >= 90 ? 'compliant' : score >= 70 ? 'partial' : 'non-compliant';

    return {
      score,
      status,
      passedChecks,
      failedChecks,
      recommendations
    };
  };

  const renderTemplateCard = (template: ProcessTemplate) => (
    <div key={template.id} className="template-card">
      <div className="template-header">
        <h4>{template.name}</h4>
        <div className="template-meta">
          <span className="itil-area">{template.itilArea}</span>
          <span 
            className="maturity-badge"
            style={{ backgroundColor: getMaturityColor(template.maturityLevel) }}
          >
            {template.maturityLevel}
          </span>
        </div>
      </div>

      <div className="template-content">
        <p className="template-description">{template.description}</p>
        
        <div className="template-stats">
          <div className="stat">
            <span className="icon">📋</span>
            <span>{template.defaultSteps.length} trinn</span>
          </div>
          <div className="stat">
            <span className="icon">⏱️</span>
            <span>{template.estimatedDuration} min</span>
          </div>
          <div className="stat">
            <span className="icon">👥</span>
            <span>{template.roles.length} roller</span>
          </div>
          <div className="stat">
            <span className="icon">📊</span>
            <span>{template.kpis.length} KPI-er</span>
          </div>
        </div>

        <div className="template-activities">
          <h6>Nøkkelaktiviteter:</h6>
          <ul>
            {template.keyActivities.slice(0, 3).map((activity, index) => (
              <li key={index}>{activity}</li>
            ))}
            {template.keyActivities.length > 3 && (
              <li>+{template.keyActivities.length - 3} flere...</li>
            )}
          </ul>
        </div>
      </div>

      <div className="template-actions">
        <button 
          className="select-btn"
          onClick={() => onTemplateSelect(template)}
        >
          Velg mal
        </button>
        <button 
          className="preview-btn"
          onClick={() => setSelectedTemplate(template)}
        >
          Forhåndsvis
        </button>
      </div>
    </div>
  );

  return (
    <div className="process-template-manager">
      <div className="template-header">
        <h3>📋 Prosessmaler og ITIL-samsvar</h3>
        <div className="tab-navigation">
          <button 
            className={`tab ${activeTab === 'browse' ? 'active' : ''}`}
            onClick={() => setActiveTab('browse')}
          >
            🔍 Bla gjennom maler
          </button>
          <button 
            className={`tab ${activeTab === 'compliance' ? 'active' : ''}`}
            onClick={() => setActiveTab('compliance')}
          >
            ✅ Samsvarssjeking
          </button>
          <button 
            className={`tab ${activeTab === 'custom' ? 'active' : ''}`}
            onClick={() => setActiveTab('custom')}
          >
            🛠️ Egendefinerte maler
          </button>
        </div>
      </div>

      {activeTab === 'browse' && (
        <div className="browse-content">
          <div className="filters-section">
            <div className="search-bar">
              <input
                type="text"
                placeholder="Søk i maler..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            
            <div className="filters">
              <select 
                value={filterITIL} 
                onChange={(e) => setFilterITIL(e.target.value)}
              >
                <option value="">Alle ITIL-områder</option>
                <option value="Service Strategy">Service Strategy</option>
                <option value="Service Design">Service Design</option>
                <option value="Service Transition">Service Transition</option>
                <option value="Service Operation">Service Operation</option>
                <option value="Continual Service Improvement">Continual Service Improvement</option>
              </select>

              <select 
                value={filterMaturity} 
                onChange={(e) => setFilterMaturity(e.target.value)}
              >
                <option value="">Alle modenhetsnivaer</option>
                <option value="Basic">Grunnleggende</option>
                <option value="Intermediate">Middels</option>
                <option value="Advanced">Avansert</option>
              </select>
            </div>
          </div>

          {loadingTemplates ? (
            <div className="loading-templates">
              <div className="spinner"></div>
              <p>Laster prosessmaler...</p>
            </div>
          ) : (
            <div className="templates-grid">
              {filteredTemplates.map(renderTemplateCard)}
            </div>
          )}
        </div>
      )}

      {activeTab === 'compliance' && selectedTemplate && (
        <div className="compliance-content">
          <h4>🔍 ITIL-samsvarssjeking for {selectedTemplate.name}</h4>
          
          {(() => {
            const complianceResult = validateProcessCompliance(selectedTemplate);
            return (
              <div className="compliance-report">
                <div className="compliance-score">
                  <div className={`score-circle ${complianceResult.status}`}>
                    <span>{Math.round(complianceResult.score)}%</span>
                  </div>
                  <div className="score-details">
                    <h5>Samsvarsscore</h5>
                    <p>
                      {complianceResult.status === 'compliant' && '✅ Fullt samsvar'}
                      {complianceResult.status === 'partial' && '⚠️ Delvis samsvar'}  
                      {complianceResult.status === 'non-compliant' && '❌ Ikke i samsvar'}
                    </p>
                  </div>
                </div>

                <div className="compliance-details">
                  <div className="passed-checks">
                    <h6>✅ Oppfylte krav ({complianceResult.passedChecks.length})</h6>
                    {complianceResult.passedChecks.map(check => (
                      <div key={check.id} className="compliance-check passed">
                        <span className="check-icon">✅</span>
                        <div>
                          <strong>{check.description}</strong>
                          <p>{check.details}</p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {complianceResult.failedChecks.length > 0 && (
                    <div className="failed-checks">
                      <h6>❌ Ikke oppfylte krav ({complianceResult.failedChecks.length})</h6>
                      {complianceResult.failedChecks.map(check => (
                        <div key={check.id} className="compliance-check failed">
                          <span className="check-icon">❌</span>
                          <div>
                            <strong>{check.description}</strong>
                            <p>{check.details}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {complianceResult.recommendations.length > 0 && (
                    <div className="recommendations">
                      <h6>💡 Anbefalinger for forbedring</h6>
                      <ul>
                        {complianceResult.recommendations.map((rec, index) => (
                          <li key={index}>{rec}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
              </div>
            );
          })()}
        </div>
      )}

      {activeTab === 'custom' && (
        <div className="custom-content">
          <h4>🛠️ Egendefinerte prosessmaler</h4>
          <div className="custom-templates-info">
            <p>Her kan du administrere egendefinerte prosessmaler tilpasset din organisasjon.</p>
            
            <div className="custom-actions">
              <button className="create-template-btn">
                ➕ Opprett ny mal
              </button>
              <button className="import-template-btn">
                📥 Importer mal
              </button>
              <button className="export-template-btn">
                📤 Eksporter maler
              </button>
            </div>
            
            <div className="coming-soon">
              <p>🚧 Egendefinerte maler kommer i neste versjon</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};