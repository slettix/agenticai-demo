import React, { useState, useEffect } from 'react';
import { Role, CreateRole, UpdateRole, RoleCategory, OrganizationLevel, Permission } from '../../types/role.ts';
import { roleService } from '../../services/roleService.ts';

interface RoleFormProps {
  role?: Role; // If provided, this is an edit form
  onSubmit: (data: CreateRole | UpdateRole) => void;
  onCancel: () => void;
  loading: boolean;
  isEdit?: boolean;
}

export const RoleForm: React.FC<RoleFormProps> = ({
  role,
  onSubmit,
  onCancel,
  loading,
  isEdit = false
}) => {
  const [formData, setFormData] = useState<CreateRole | UpdateRole>({
    name: '',
    description: '',
    category: RoleCategory.Internal,
    level: OrganizationLevel.Individual,
    permissionIds: [],
    ...(isEdit && { isActive: true })
  });

  const [availablePermissions, setAvailablePermissions] = useState<Permission[]>([]);
  const [selectedPermissions, setSelectedPermissions] = useState<Set<number>>(new Set());
  const [loadingPermissions, setLoadingPermissions] = useState(false);

  useEffect(() => {
    loadPermissions();
  }, []);

  useEffect(() => {
    if (role) {
      setFormData({
        name: role.name,
        description: role.description,
        category: role.category,
        level: role.level,
        permissionIds: role.permissions?.map(p => p.id) || [],
        ...(isEdit && { isActive: role.isActive })
      });
      
      const permissionIds = new Set(role.permissions?.map(p => p.id) || []);
      setSelectedPermissions(permissionIds);
    }
  }, [role, isEdit]);

  const loadPermissions = async () => {
    try {
      setLoadingPermissions(true);
      const permissions = await roleService.getAllPermissions();
      setAvailablePermissions(permissions);
    } catch (error) {
      console.error('Kunne ikke laste tilganger:', error);
    } finally {
      setLoadingPermissions(false);
    }
  };

  const handleInputChange = (field: keyof (CreateRole | UpdateRole), value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handlePermissionToggle = (permissionId: number) => {
    const newSelected = new Set(selectedPermissions);
    if (newSelected.has(permissionId)) {
      newSelected.delete(permissionId);
    } else {
      newSelected.add(permissionId);
    }
    setSelectedPermissions(newSelected);
    setFormData(prev => ({ 
      ...prev, 
      permissionIds: Array.from(newSelected)
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const getRoleCategoryLabel = (category: RoleCategory): string => {
    switch (category) {
      case RoleCategory.Internal: return 'Intern';
      case RoleCategory.External: return 'Ekstern';
      case RoleCategory.System: return 'System';
      default: return 'Ukjent';
    }
  };

  const getOrganizationLevelLabel = (level: OrganizationLevel): string => {
    switch (level) {
      case OrganizationLevel.Individual: return 'Individuell';
      case OrganizationLevel.Unit: return 'Enhet';
      case OrganizationLevel.Department: return 'Avdeling';
      case OrganizationLevel.Organization: return 'Organisasjon';
      case OrganizationLevel.National: return 'Nasjonalt';
      default: return 'Ukjent';
    }
  };

  const groupPermissionsByResource = (permissions: Permission[]) => {
    return permissions.reduce((acc, permission) => {
      if (!acc[permission.resource]) {
        acc[permission.resource] = [];
      }
      acc[permission.resource].push(permission);
      return acc;
    }, {} as Record<string, Permission[]>);
  };

  const groupedPermissions = groupPermissionsByResource(availablePermissions);
  const resourceNames = Object.keys(groupedPermissions).sort();

  return (
    <div className="role-form">
      <h2>{isEdit ? 'Rediger Rolle' : 'Ny Rolle'}</h2>
      
      <form onSubmit={handleSubmit} className="form">
        <div className="form-section">
          <h3>Grunnleggende informasjon</h3>
          
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="name">Rollenavn *</label>
              <input
                id="name"
                type="text"
                value={formData.name}
                onChange={(e) => handleInputChange('name', e.target.value)}
                required
                className="form-input"
                placeholder="F.eks. Prosessansvarlig"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group full-width">
              <label htmlFor="description">Beskrivelse *</label>
              <textarea
                id="description"
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                required
                className="form-textarea"
                rows={3}
                placeholder="Beskriv rollens ansvar og oppgaver..."
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="category">Kategori *</label>
              <select
                id="category"
                value={formData.category}
                onChange={(e) => handleInputChange('category', Number(e.target.value))}
                required
                className="form-select"
              >
                <option value={RoleCategory.Internal}>{getRoleCategoryLabel(RoleCategory.Internal)}</option>
                <option value={RoleCategory.External}>{getRoleCategoryLabel(RoleCategory.External)}</option>
                <option value={RoleCategory.System}>{getRoleCategoryLabel(RoleCategory.System)}</option>
              </select>
              <small className="form-help">
                {formData.category === RoleCategory.Internal && 'Interne roller for Forsvaret ansatte'}
                {formData.category === RoleCategory.External && 'Eksterne roller for kontraktører og partnere'}
                {formData.category === RoleCategory.System && 'Systemroller for tekniske operasjoner'}
              </small>
            </div>

            <div className="form-group">
              <label htmlFor="level">Organisasjonsnivå *</label>
              <select
                id="level"
                value={formData.level}
                onChange={(e) => handleInputChange('level', Number(e.target.value))}
                required
                className="form-select"
              >
                <option value={OrganizationLevel.Individual}>{getOrganizationLevelLabel(OrganizationLevel.Individual)}</option>
                <option value={OrganizationLevel.Unit}>{getOrganizationLevelLabel(OrganizationLevel.Unit)}</option>
                <option value={OrganizationLevel.Department}>{getOrganizationLevelLabel(OrganizationLevel.Department)}</option>
                <option value={OrganizationLevel.Organization}>{getOrganizationLevelLabel(OrganizationLevel.Organization)}</option>
                <option value={OrganizationLevel.National}>{getOrganizationLevelLabel(OrganizationLevel.National)}</option>
              </select>
              <small className="form-help">
                {formData.level === OrganizationLevel.Individual && 'Rolle gjelder for individuelle brukere'}
                {formData.level === OrganizationLevel.Unit && 'Rolle gjelder for enhets-nivå'}
                {formData.level === OrganizationLevel.Department && 'Rolle gjelder for avdeling-nivå'}
                {formData.level === OrganizationLevel.Organization && 'Rolle gjelder for organisasjon-nivå'}
                {formData.level === OrganizationLevel.National && 'Rolle gjelder for nasjonalt nivå'}
              </small>
            </div>
          </div>

          {isEdit && (
            <div className="form-row">
              <div className="form-group">
                <label className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={(formData as UpdateRole).isActive}
                    onChange={(e) => handleInputChange('isActive', e.target.checked)}
                  />
                  Aktiv rolle
                </label>
              </div>
            </div>
          )}
        </div>

        <div className="form-section">
          <h3>Tilganger og rettigheter</h3>
          
          {loadingPermissions ? (
            <div className="loading-permissions">
              <p>Laster tilganger...</p>
            </div>
          ) : (
            <div className="permissions-section">
              <div className="permissions-summary">
                <p>
                  Valgte tilganger: {selectedPermissions.size} av {availablePermissions.length}
                </p>
              </div>

              {resourceNames.length === 0 ? (
                <div className="no-permissions">
                  <p>Ingen tilganger er konfigurert i systemet.</p>
                </div>
              ) : (
                <div className="permissions-grid">
                  {resourceNames.map(resource => (
                    <div key={resource} className="permission-group">
                      <h4 className="permission-resource">{resource}</h4>
                      <div className="permission-actions">
                        {groupedPermissions[resource].map(permission => (
                          <label key={permission.id} className="permission-item">
                            <input
                              type="checkbox"
                              checked={selectedPermissions.has(permission.id)}
                              onChange={() => handlePermissionToggle(permission.id)}
                            />
                            <span className="permission-info">
                              <strong>{permission.action}</strong>
                              <small>{permission.description}</small>
                            </span>
                          </label>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        <div className="form-actions">
          <button type="button" onClick={onCancel} className="btn btn-secondary" disabled={loading}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Lagrer...' : (isEdit ? 'Oppdater rolle' : 'Opprett rolle')}
          </button>
        </div>
      </form>
    </div>
  );
};