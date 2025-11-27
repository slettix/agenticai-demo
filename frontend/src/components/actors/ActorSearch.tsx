import React, { useState, useEffect } from 'react';
import { ActorSearch as ActorSearchType, ActorCategory, ActorType, SecurityClearance, actorService } from '../../services/actorService.ts';

export { ActorCategory, ActorType, SecurityClearance };

interface ActorSearchProps {
  onSearch: (searchCriteria: ActorSearchType) => void;
  loading: boolean;
  currentCriteria: ActorSearchType;
}

export const ActorSearch: React.FC<ActorSearchProps> = ({
  onSearch,
  loading,
  currentCriteria
}) => {
  const [searchForm, setSearchForm] = useState<ActorSearchType>(currentCriteria);
  const [organizations, setOrganizations] = useState<string[]>([]);
  const [departments, setDepartments] = useState<string[]>([]);
  const [showAdvanced, setShowAdvanced] = useState(false);

  useEffect(() => {
    loadFilterOptions();
  }, []);

  const loadFilterOptions = async () => {
    try {
      const [orgs, depts] = await Promise.all([
        actorService.getOrganizations(),
        actorService.getDepartments()
      ]);
      setOrganizations(orgs);
      setDepartments(depts);
    } catch (error) {
      console.error('Kunne ikke laste filteralternativer:', error);
    }
  };

  const handleInputChange = (field: keyof ActorSearchType, value: any) => {
    setSearchForm(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch(searchForm);
  };

  const handleReset = () => {
    const resetForm: ActorSearchType = {
      page: 1,
      pageSize: 20,
      isActive: true
    };
    setSearchForm(resetForm);
    onSearch(resetForm);
  };

  const getActorCategoryLabel = (category: ActorCategory): string => {
    switch (category) {
      case ActorCategory.Person: return 'Person';
      case ActorCategory.Organization: return 'Organisasjon';
      case ActorCategory.Unit: return 'Enhet';
      default: return 'Ukjent';
    }
  };

  const getActorTypeLabel = (type: ActorType): string => {
    switch (type) {
      case ActorType.Internal: return 'Intern (Forsvaret)';
      case ActorType.External: return 'Ekstern';
      case ActorType.Contractor: return 'Konsulent';
      case ActorType.Partner: return 'Partner';
      case ActorType.Vendor: return 'Leverandør';
      default: return 'Ukjent';
    }
  };

  const getSecurityClearanceLabel = (clearance: SecurityClearance): string => {
    switch (clearance) {
      case SecurityClearance.None: return 'Ingen';
      case SecurityClearance.Restricted: return 'Begrenset';
      case SecurityClearance.Confidential: return 'Konfidensielt';
      case SecurityClearance.Secret: return 'Hemmelig';
      case SecurityClearance.TopSecret: return 'Strengt hemmelig';
      default: return 'Ukjent';
    }
  };

  return (
    <div className="actor-search">
      <form onSubmit={handleSubmit} className="search-form">
        <div className="search-row">
          <div className="search-field">
            <input
              type="text"
              placeholder="Søk etter navn, e-post, organisasjon..."
              value={searchForm.searchTerm || ''}
              onChange={(e) => handleInputChange('searchTerm', e.target.value)}
              className="search-input"
            />
          </div>
          
          <div className="search-field">
            <select
              value={searchForm.actorCategory ?? ''}
              onChange={(e) => handleInputChange('actorCategory', e.target.value ? Number(e.target.value) : undefined)}
              className="search-select"
            >
              <option value="">Alle kategorier</option>
              {Object.values(ActorCategory).filter(value => typeof value === 'number').map(category => (
                <option key={category} value={category}>
                  {getActorCategoryLabel(category as ActorCategory)}
                </option>
              ))}
            </select>
          </div>

          <div className="search-field">
            <select
              value={searchForm.actorType ?? ''}
              onChange={(e) => handleInputChange('actorType', e.target.value ? Number(e.target.value) : undefined)}
              className="search-select"
            >
              <option value="">Alle aktørtyper</option>
              {Object.values(ActorType).filter(value => typeof value === 'number').map(type => (
                <option key={type} value={type}>
                  {getActorTypeLabel(type as ActorType)}
                </option>
              ))}
            </select>
          </div>

          <div className="search-field">
            <select
              value={searchForm.isActive !== undefined ? searchForm.isActive.toString() : ''}
              onChange={(e) => handleInputChange('isActive', e.target.value === '' ? undefined : e.target.value === 'true')}
              className="search-select"
            >
              <option value="">Alle statuser</option>
              <option value="true">Aktive</option>
              <option value="false">Inaktive</option>
            </select>
          </div>

          <div className="search-actions">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Søker...' : 'Søk'}
            </button>
            <button 
              type="button" 
              className="btn btn-secondary"
              onClick={() => setShowAdvanced(!showAdvanced)}
            >
              {showAdvanced ? 'Enkel søk' : 'Avansert søk'}
            </button>
          </div>
        </div>

        {showAdvanced && (
          <div className="advanced-search">
            <div className="search-row">
              <div className="search-field">
                <label>Sikkerhetsnivå:</label>
                <select
                  value={searchForm.securityClearance ?? ''}
                  onChange={(e) => handleInputChange('securityClearance', e.target.value ? Number(e.target.value) : undefined)}
                  className="search-select"
                >
                  <option value="">Alle sikkerhetsnivå</option>
                  {Object.values(SecurityClearance).filter(value => typeof value === 'number').map(clearance => (
                    <option key={clearance} value={clearance}>
                      {getSecurityClearanceLabel(clearance as SecurityClearance)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="search-field">
                <label>Organisasjon:</label>
                <select
                  value={searchForm.organizationName || ''}
                  onChange={(e) => handleInputChange('organizationName', e.target.value || undefined)}
                  className="search-select"
                >
                  <option value="">Alle organisasjoner</option>
                  {organizations.map(org => (
                    <option key={org} value={org}>{org}</option>
                  ))}
                </select>
              </div>

              <div className="search-field">
                <label>Enhet:</label>
                <input
                  type="text"
                  placeholder="F.eks. Cyber Brigade, 2. Bataljon..."
                  value={searchForm.unitName || ''}
                  onChange={(e) => handleInputChange('unitName', e.target.value || undefined)}
                  className="search-input"
                />
              </div>

              <div className="search-field">
                <label>Avdeling:</label>
                <select
                  value={searchForm.department || ''}
                  onChange={(e) => handleInputChange('department', e.target.value || undefined)}
                  className="search-select"
                >
                  <option value="">Alle avdelinger</option>
                  {departments.map(dept => (
                    <option key={dept} value={dept}>{dept}</option>
                  ))}
                </select>
              </div>

              <div className="search-field">
                <label>Geografisk lokasjon:</label>
                <input
                  type="text"
                  placeholder="F.eks. Oslo, Bergen..."
                  value={searchForm.geographicLocation || ''}
                  onChange={(e) => handleInputChange('geographicLocation', e.target.value || undefined)}
                  className="search-input"
                />
              </div>
            </div>


            <div className="advanced-actions">
              <button type="button" className="btn btn-link" onClick={handleReset}>
                Nullstill søk
              </button>
            </div>
          </div>
        )}
      </form>
    </div>
  );
};