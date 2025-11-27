import React, { useState, useEffect } from 'react';
import { Actor, CreateActor, UpdateActor, ActorCategory, ActorType, SecurityClearance, actorService } from '../../services/actorService.ts';

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
    actorCategory: ActorCategory.Person,
    firstName: '',
    lastName: '',
    organizationName: '',
    unitName: '',
    unitType: '',
    unitCode: '',
    email: '',
    phone: '',
    actorType: ActorType.Internal,
    securityClearance: SecurityClearance.None,
    department: '',
    position: '',
    managerName: '',
    managerEmail: '',
    geographicLocation: '',
    address: '',
    preferredLanguage: 'NO',
    contractNumber: '',
    contractStartDate: '',
    contractEndDate: '',
    vendorId: '',
    registrationNumber: '',
    parentOrganization: '',
    employeeCount: undefined,
    commandStructure: '',
    unitMission: '',
    personnelCount: undefined,
    ...(isEdit && { isActive: true })
  });

  const [availableOrganizations, setAvailableOrganizations] = useState<string[]>([]);
  const [availableDepartments, setAvailableDepartments] = useState<string[]>([]);

  useEffect(() => {
    loadOptions();
  }, []);

  useEffect(() => {
    if (actor) {
      setFormData({
        actorCategory: actor.actorCategory,
        firstName: actor.firstName || '',
        lastName: actor.lastName || '',
        organizationName: actor.organizationName || '',
        unitName: actor.unitName || '',
        unitType: actor.unitType || '',
        unitCode: actor.unitCode || '',
        email: actor.email,
        phone: actor.phone || '',
        actorType: actor.actorType,
        securityClearance: actor.securityClearance,
        department: actor.department || '',
        position: actor.position || '',
        managerName: actor.managerName || '',
        managerEmail: actor.managerEmail || '',
        geographicLocation: actor.geographicLocation || '',
        address: actor.address || '',
        preferredLanguage: actor.preferredLanguage || 'NO',
        contractNumber: actor.contractNumber || '',
        contractStartDate: actor.contractStartDate ? actor.contractStartDate.split('T')[0] : '',
        contractEndDate: actor.contractEndDate ? actor.contractEndDate.split('T')[0] : '',
        vendorId: actor.vendorId || '',
        registrationNumber: actor.registrationNumber || '',
        parentOrganization: actor.parentOrganization || '',
        employeeCount: actor.employeeCount,
        commandStructure: actor.commandStructure || '',
        unitMission: actor.unitMission || '',
        personnelCount: actor.personnelCount,
        ...(isEdit && { isActive: actor.isActive })
      });
    }
  }, [actor, isEdit]);

  const loadOptions = async () => {
    try {
      const [organizations, departments] = await Promise.all([
        actorService.getOrganizations(),
        actorService.getDepartments()
      ]);
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

  const isExternal = formData.actorType !== ActorType.Internal;

  return (
    <div className="actor-form">
      <h2>{isEdit ? 'Rediger Aktør' : 'Ny Aktør'}</h2>
      
      <form onSubmit={handleSubmit} className="form">
        <div className="form-section">
          <h3>Grunnleggende informasjon</h3>
          
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="actorCategory">Aktørkategori *</label>
              <select
                id="actorCategory"
                value={formData.actorCategory}
                onChange={(e) => handleInputChange('actorCategory', Number(e.target.value))}
                required
                className="form-select"
              >
                <option value={ActorCategory.Person}>Person</option>
                <option value={ActorCategory.Organization}>Organisasjon</option>
                <option value={ActorCategory.Unit}>Enhet</option>
              </select>
            </div>
          </div>

          {formData.actorCategory === ActorCategory.Person && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="firstName">Fornavn *</label>
                <input
                  id="firstName"
                  type="text"
                  value={formData.firstName || ''}
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
                  value={formData.lastName || ''}
                  onChange={(e) => handleInputChange('lastName', e.target.value)}
                  required
                  className="form-input"
                />
              </div>
            </div>
          )}

          {formData.actorCategory === ActorCategory.Organization && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="organizationName">Organisasjonsnavn *</label>
                <input
                  id="organizationName"
                  type="text"
                  value={formData.organizationName || ''}
                  onChange={(e) => handleInputChange('organizationName', e.target.value)}
                  required
                  className="form-input"
                  placeholder="F.eks. TechCorp AS"
                />
              </div>

              <div className="form-group">
                <label htmlFor="registrationNumber">Organisasjonsnummer</label>
                <input
                  id="registrationNumber"
                  type="text"
                  value={formData.registrationNumber || ''}
                  onChange={(e) => handleInputChange('registrationNumber', e.target.value)}
                  className="form-input"
                  placeholder="123456789"
                />
              </div>
            </div>
          )}

          {formData.actorCategory === ActorCategory.Unit && (
            <>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="unitName">Enhetsnavn *</label>
                  <input
                    id="unitName"
                    type="text"
                    value={formData.unitName || ''}
                    onChange={(e) => handleInputChange('unitName', e.target.value)}
                    required
                    className="form-input"
                    placeholder="F.eks. Cyber Brigade"
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="unitCode">Enhetskode</label>
                  <input
                    id="unitCode"
                    type="text"
                    value={formData.unitCode || ''}
                    onChange={(e) => handleInputChange('unitCode', e.target.value)}
                    className="form-input"
                    placeholder="F.eks. CYB-BDE"
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="unitType">Enhetstype</label>
                  <select
                    id="unitType"
                    value={formData.unitType || ''}
                    onChange={(e) => handleInputChange('unitType', e.target.value)}
                    className="form-select"
                  >
                    <option value="">Velg enhetstype</option>
                    <option value="Brigade">Brigade</option>
                    <option value="Bataljon">Bataljon</option>
                    <option value="Kompani">Kompani</option>
                    <option value="Tropp">Tropp</option>
                    <option value="Gruppe">Gruppe</option>
                    <option value="Avdeling">Avdeling</option>
                    <option value="Seksjon">Seksjon</option>
                  </select>
                </div>

                <div className="form-group">
                  <label htmlFor="commandStructure">Kommandostruktur</label>
                  <input
                    id="commandStructure"
                    type="text"
                    value={formData.commandStructure || ''}
                    onChange={(e) => handleInputChange('commandStructure', e.target.value)}
                    className="form-input"
                    placeholder="F.eks. Hærstaben"
                  />
                </div>
              </div>
            </>
          )}

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
          <h3>
            {formData.actorCategory === ActorCategory.Organization && 'Organisasjonsdetaljer'}
            {formData.actorCategory === ActorCategory.Unit && 'Enhetsdetaljer'}
            {formData.actorCategory === ActorCategory.Person && 'Organisasjon og rolle'}
          </h3>
          
          {formData.actorCategory === ActorCategory.Person && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="organizationName">Organisasjon</label>
                <input
                  id="organizationName"
                  type="text"
                  value={formData.organizationName || ''}
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
                  value={formData.department || ''}
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
          )}

          {formData.actorCategory === ActorCategory.Organization && (
            <>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="parentOrganization">Moderorganisasjon</label>
                  <input
                    id="parentOrganization"
                    type="text"
                    value={formData.parentOrganization || ''}
                    onChange={(e) => handleInputChange('parentOrganization', e.target.value)}
                    className="form-input"
                    placeholder="F.eks. TechCorp International"
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="employeeCount">Antall ansatte</label>
                  <input
                    id="employeeCount"
                    type="number"
                    min="1"
                    value={formData.employeeCount || ''}
                    onChange={(e) => handleInputChange('employeeCount', e.target.value ? parseInt(e.target.value) : undefined)}
                    className="form-input"
                  />
                </div>
              </div>
            </>
          )}

          {formData.actorCategory === ActorCategory.Unit && (
            <>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="unitMission">Enhetsoppdrag</label>
                  <textarea
                    id="unitMission"
                    value={formData.unitMission || ''}
                    onChange={(e) => handleInputChange('unitMission', e.target.value)}
                    className="form-textarea"
                    rows={3}
                    placeholder="Beskriv enhetens hovedoppdrag og ansvar..."
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="personnelCount">Personalstyrke</label>
                  <input
                    id="personnelCount"
                    type="number"
                    min="1"
                    value={formData.personnelCount || ''}
                    onChange={(e) => handleInputChange('personnelCount', e.target.value ? parseInt(e.target.value) : undefined)}
                    className="form-input"
                  />
                </div>
              </div>
            </>
          )}

          <div className="form-row">
            {formData.actorCategory === ActorCategory.Person && (
              <div className="form-group">
                <label htmlFor="position">Stilling</label>
                <input
                  id="position"
                  type="text"
                  value={formData.position || ''}
                  onChange={(e) => handleInputChange('position', e.target.value)}
                  className="form-input"
                />
              </div>
            )}

            <div className="form-group">
              <label htmlFor="geographicLocation">Geografisk lokasjon</label>
              <input
                id="geographicLocation"
                type="text"
                value={formData.geographicLocation || ''}
                onChange={(e) => handleInputChange('geographicLocation', e.target.value)}
                className="form-input"
                placeholder="F.eks. Oslo, Bergen, Trondheim"
              />
            </div>
          </div>

          {formData.actorCategory === ActorCategory.Person && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="managerName">Leder</label>
                <input
                  id="managerName"
                  type="text"
                  value={formData.managerName || ''}
                  onChange={(e) => handleInputChange('managerName', e.target.value)}
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label htmlFor="managerEmail">Leder e-post</label>
                <input
                  id="managerEmail"
                  type="email"
                  value={formData.managerEmail || ''}
                  onChange={(e) => handleInputChange('managerEmail', e.target.value)}
                  className="form-input"
                />
              </div>
            </div>
          )}

          {formData.actorCategory !== ActorCategory.Person && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="department">Avdeling/Underenhet</label>
                <input
                  id="department"
                  type="text"
                  value={formData.department || ''}
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
          )}
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
          <h3>Språk og kontakt</h3>

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