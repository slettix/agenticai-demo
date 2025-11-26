import React, { useState, useEffect } from 'react';
import { Actor, CreateActor, UpdateActor, ActorType, SecurityClearance, actorService } from '../../services/actorService.ts';

interface ActorFormProps {
  actor?: Actor; // If provided, this is an edit form
  onSubmit: (data: CreateActor | UpdateActor) => void;
  onCancel: () => void;
  loading: boolean;
  isEdit?: boolean;
}

export const ActorForm: React.FC<ActorFormProps> = ({
  actor,
  onSubmit,
  onCancel,
  loading,
  isEdit = false
}) => {
  const [formData, setFormData] = useState<CreateActor | UpdateActor>({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    actorType: ActorType.Internal,
    securityClearance: SecurityClearance.None,
    organizationName: '',
    department: '',
    position: '',
    managerName: '',
    managerEmail: '',
    geographicLocation: '',
    address: '',
    preferredLanguage: 'NO',
    competenceAreas: [],
    technicalSkills: [],
    contractNumber: '',
    contractStartDate: '',
    contractEndDate: '',
    vendorId: '',
    ...(isEdit && { isActive: true })
  });

  const [availableCompetences, setAvailableCompetences] = useState<string[]>([]);
  const [availableSkills, setAvailableSkills] = useState<string[]>([]);
  const [availableOrganizations, setAvailableOrganizations] = useState<string[]>([]);
  const [availableDepartments, setAvailableDepartments] = useState<string[]>([]);

  const [newCompetence, setNewCompetence] = useState('');
  const [newSkill, setNewSkill] = useState('');

  useEffect(() => {
    loadOptions();
  }, []);

  useEffect(() => {
    if (actor) {
      setFormData({
        firstName: actor.firstName,
        lastName: actor.lastName,
        email: actor.email,
        phone: actor.phone || '',
        actorType: actor.actorType,
        securityClearance: actor.securityClearance,
        organizationName: actor.organizationName || '',
        department: actor.department || '',
        position: actor.position || '',
        managerName: actor.managerName || '',
        managerEmail: actor.managerEmail || '',
        geographicLocation: actor.geographicLocation || '',
        address: actor.address || '',
        preferredLanguage: actor.preferredLanguage || 'NO',
        competenceAreas: actor.competenceAreas || [],
        technicalSkills: actor.technicalSkills || [],
        contractNumber: actor.contractNumber || '',
        contractStartDate: actor.contractStartDate ? actor.contractStartDate.split('T')[0] : '',
        contractEndDate: actor.contractEndDate ? actor.contractEndDate.split('T')[0] : '',
        vendorId: actor.vendorId || '',
        ...(isEdit && { isActive: actor.isActive })
      });
    }
  }, [actor, isEdit]);

  const loadOptions = async () => {
    try {
      const [competences, skills, organizations, departments] = await Promise.all([
        actorService.getCompetenceAreas(),
        actorService.getTechnicalSkills(),
        actorService.getOrganizations(),
        actorService.getDepartments()
      ]);
      setAvailableCompetences(competences);
      setAvailableSkills(skills);
      setAvailableOrganizations(organizations);
      setAvailableDepartments(departments);
    } catch (error) {
      console.error('Kunne ikke laste valgalternativer:', error);
    }
  };

  const handleInputChange = (field: keyof (CreateActor | UpdateActor), value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const addCompetence = () => {
    if (newCompetence.trim() && !formData.competenceAreas?.includes(newCompetence.trim())) {
      const updatedCompetences = [...(formData.competenceAreas || []), newCompetence.trim()];
      handleInputChange('competenceAreas', updatedCompetences);
      setNewCompetence('');
    }
  };

  const removeCompetence = (competence: string) => {
    const updatedCompetences = formData.competenceAreas?.filter(c => c !== competence) || [];
    handleInputChange('competenceAreas', updatedCompetences);
  };

  const addSkill = () => {
    if (newSkill.trim() && !formData.technicalSkills?.includes(newSkill.trim())) {
      const updatedSkills = [...(formData.technicalSkills || []), newSkill.trim()];
      handleInputChange('technicalSkills', updatedSkills);
      setNewSkill('');
    }
  };

  const removeSkill = (skill: string) => {
    const updatedSkills = formData.technicalSkills?.filter(s => s !== skill) || [];
    handleInputChange('technicalSkills', updatedSkills);
  };

  const isExternal = formData.actorType !== ActorType.Internal;

  return (
    <div className="actor-form">
      <h2>{isEdit ? 'Rediger Aktør' : 'Ny Aktør'}</h2>
      
      <form onSubmit={handleSubmit} className="form">
        <div className="form-section">
          <h3>Grunnleggende informasjon</h3>
          
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">Fornavn *</label>
              <input
                id="firstName"
                type="text"
                value={formData.firstName}
                onChange={(e) => handleInputChange('firstName', e.target.value)}
                required
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Etternavn *</label>
              <input
                id="lastName"
                type="text"
                value={formData.lastName}
                onChange={(e) => handleInputChange('lastName', e.target.value)}
                required
                className="form-input"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="email">E-post *</label>
              <input
                id="email"
                type="email"
                value={formData.email}
                onChange={(e) => handleInputChange('email', e.target.value)}
                required
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label htmlFor="phone">Telefon</label>
              <input
                id="phone"
                type="tel"
                value={formData.phone}
                onChange={(e) => handleInputChange('phone', e.target.value)}
                className="form-input"
                placeholder="+47 12 34 56 78"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="actorType">Aktørtype *</label>
              <select
                id="actorType"
                value={formData.actorType}
                onChange={(e) => handleInputChange('actorType', Number(e.target.value))}
                required
                className="form-select"
              >
                <option value={ActorType.Internal}>Intern (Forsvaret)</option>
                <option value={ActorType.External}>Ekstern</option>
                <option value={ActorType.Contractor}>Konsulent</option>
                <option value={ActorType.Partner}>Partner</option>
                <option value={ActorType.Vendor}>Leverandør</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="securityClearance">Sikkerhetsnivå</label>
              <select
                id="securityClearance"
                value={formData.securityClearance}
                onChange={(e) => handleInputChange('securityClearance', Number(e.target.value))}
                className="form-select"
              >
                <option value={SecurityClearance.None}>Ingen</option>
                <option value={SecurityClearance.Restricted}>Begrenset</option>
                <option value={SecurityClearance.Confidential}>Konfidensielt</option>
                <option value={SecurityClearance.Secret}>Hemmelig</option>
                <option value={SecurityClearance.TopSecret}>Strengt hemmelig</option>
              </select>
            </div>
          </div>

          {isEdit && (
            <div className="form-row">
              <div className="form-group">
                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={(formData as UpdateActor).isActive}
                    onChange={(e) => handleInputChange('isActive', e.target.checked)}
                  />
                  Aktiv aktør
                </label>
              </div>
            </div>
          )}
        </div>

        <div className="form-section">
          <h3>Organisasjon og rolle</h3>
          
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="organizationName">Organisasjon</label>
              <input
                id="organizationName"
                type="text"
                value={formData.organizationName}
                onChange={(e) => handleInputChange('organizationName', e.target.value)}
                className="form-input"
                list="organizations"
                placeholder={isExternal ? "F.eks. TechCorp AS" : "Forsvaret"}
              />
              <datalist id="organizations">
                {availableOrganizations.map(org => (
                  <option key={org} value={org} />
                ))}
              </datalist>
            </div>

            <div className="form-group">
              <label htmlFor="department">Avdeling</label>
              <input
                id="department"
                type="text"
                value={formData.department}
                onChange={(e) => handleInputChange('department', e.target.value)}
                className="form-input"
                list="departments"
              />
              <datalist id="departments">
                {availableDepartments.map(dept => (
                  <option key={dept} value={dept} />
                ))}
              </datalist>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="position">Stilling</label>
              <input
                id="position"
                type="text"
                value={formData.position}
                onChange={(e) => handleInputChange('position', e.target.value)}
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label htmlFor="geographicLocation">Geografisk lokasjon</label>
              <input
                id="geographicLocation"
                type="text"
                value={formData.geographicLocation}
                onChange={(e) => handleInputChange('geographicLocation', e.target.value)}
                className="form-input"
                placeholder="F.eks. Oslo, Bergen, Trondheim"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="managerName">Leder</label>
              <input
                id="managerName"
                type="text"
                value={formData.managerName}
                onChange={(e) => handleInputChange('managerName', e.target.value)}
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label htmlFor="managerEmail">Leder e-post</label>
              <input
                id="managerEmail"
                type="email"
                value={formData.managerEmail}
                onChange={(e) => handleInputChange('managerEmail', e.target.value)}
                className="form-input"
              />
            </div>
          </div>
        </div>

        {isExternal && (
          <div className="form-section">
            <h3>Kontraktinformasjon</h3>
            
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="contractNumber">Kontraktnummer</label>
                <input
                  id="contractNumber"
                  type="text"
                  value={formData.contractNumber}
                  onChange={(e) => handleInputChange('contractNumber', e.target.value)}
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label htmlFor="vendorId">Leverandør-ID</label>
                <input
                  id="vendorId"
                  type="text"
                  value={formData.vendorId}
                  onChange={(e) => handleInputChange('vendorId', e.target.value)}
                  className="form-input"
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="contractStartDate">Kontraktstart</label>
                <input
                  id="contractStartDate"
                  type="date"
                  value={formData.contractStartDate}
                  onChange={(e) => handleInputChange('contractStartDate', e.target.value)}
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label htmlFor="contractEndDate">Kontraktslutt</label>
                <input
                  id="contractEndDate"
                  type="date"
                  value={formData.contractEndDate}
                  onChange={(e) => handleInputChange('contractEndDate', e.target.value)}
                  className="form-input"
                />
              </div>
            </div>
          </div>
        )}

        <div className="form-section">
          <h3>Kompetanse og ferdigheter</h3>
          
          <div className="form-group">
            <label>Kompetanseområder</label>
            <div className="tag-input">
              <div className="add-tag">
                <input
                  type="text"
                  value={newCompetence}
                  onChange={(e) => setNewCompetence(e.target.value)}
                  placeholder="Legg til kompetanseområde"
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addCompetence())}
                  list="available-competences"
                />
                <datalist id="available-competences">
                  {availableCompetences.map(comp => (
                    <option key={comp} value={comp} />
                  ))}
                </datalist>
                <button type="button" onClick={addCompetence} className="btn btn-sm btn-primary">
                  Legg til
                </button>
              </div>
              <div className="tags">
                {formData.competenceAreas?.map(competence => (
                  <span key={competence} className="tag">
                    {competence}
                    <button type="button" onClick={() => removeCompetence(competence)} className="tag-remove">
                      ×
                    </button>
                  </span>
                ))}
              </div>
            </div>
          </div>

          <div className="form-group">
            <label>Tekniske ferdigheter</label>
            <div className="tag-input">
              <div className="add-tag">
                <input
                  type="text"
                  value={newSkill}
                  onChange={(e) => setNewSkill(e.target.value)}
                  placeholder="Legg til teknisk ferdighet"
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addSkill())}
                  list="available-skills"
                />
                <datalist id="available-skills">
                  {availableSkills.map(skill => (
                    <option key={skill} value={skill} />
                  ))}
                </datalist>
                <button type="button" onClick={addSkill} className="btn btn-sm btn-primary">
                  Legg til
                </button>
              </div>
              <div className="tags">
                {formData.technicalSkills?.map(skill => (
                  <span key={skill} className="tag">
                    {skill}
                    <button type="button" onClick={() => removeSkill(skill)} className="tag-remove">
                      ×
                    </button>
                  </span>
                ))}
              </div>
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="preferredLanguage">Foretrukket språk</label>
            <select
              id="preferredLanguage"
              value={formData.preferredLanguage}
              onChange={(e) => handleInputChange('preferredLanguage', e.target.value)}
              className="form-select"
            >
              <option value="NO">Norsk</option>
              <option value="EN">English</option>
              <option value="SE">Svenska</option>
              <option value="DA">Dansk</option>
            </select>
          </div>
        </div>

        <div className="form-section">
          <h3>Adresse</h3>
          <div className="form-group">
            <label htmlFor="address">Adresse</label>
            <textarea
              id="address"
              value={formData.address}
              onChange={(e) => handleInputChange('address', e.target.value)}
              className="form-textarea"
              rows={3}
            />
          </div>
        </div>

        <div className="form-actions">
          <button type="button" onClick={onCancel} className="btn btn-secondary" disabled={loading}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Lagrer...' : (isEdit ? 'Oppdater aktør' : 'Opprett aktør')}
          </button>
        </div>
      </form>
    </div>
  );
};