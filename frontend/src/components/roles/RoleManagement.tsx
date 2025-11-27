import React, { useState, useEffect } from 'react';
import { Role, CreateRole, UpdateRole, RoleSearch, RoleCategory, OrganizationLevel } from '../../types/role.ts';
import { roleService } from '../../services/roleService.ts';
import { RoleList } from './RoleList.tsx';
import { RoleForm } from './RoleForm.tsx';

type ViewMode = 'list' | 'create' | 'edit' | 'view';

export const RoleManagement: React.FC = () => {
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [roles, setRoles] = useState<Role[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [selectedRole, setSelectedRole] = useState<Role | null>(null);
  const [searchParams, setSearchParams] = useState<RoleSearch>({
    searchTerm: '',
    category: undefined,
    level: undefined,
    isActive: true,
    page: 1,
    pageSize: 10
  });
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    loadRoles();
  }, [currentPage, searchParams]);

  const loadRoles = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const searchWithPaging = {
        ...searchParams,
        page: currentPage,
        pageSize
      };
      
      const result = await roleService.getRoles(searchWithPaging);
      setRoles(result.roles);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (error) {
      console.error('Feil ved lasting av roller:', error);
      setError('Kunne ikke laste roller. Prøv igjen senere.');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (newSearchParams: Partial<RoleSearch>) => {
    setCurrentPage(1);
    setSearchParams(prev => ({ ...prev, ...newSearchParams }));
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleCreateRole = () => {
    setSelectedRole(null);
    setViewMode('create');
  };

  const handleEditRole = (role: Role) => {
    setSelectedRole(role);
    setViewMode('edit');
  };

  const handleViewRole = (role: Role) => {
    setSelectedRole(role);
    setViewMode('view');
  };

  const handleDeleteRole = async (roleId: number) => {
    if (!window.confirm('Er du sikker på at du vil slette denne rollen? Dette kan ikke angres.')) {
      return;
    }

    try {
      setError(null);
      await roleService.deleteRole(roleId);
      setSuccess('Rolle slettet');
      loadRoles();
    } catch (error) {
      console.error('Feil ved sletting av rolle:', error);
      setError('Kunne ikke slette rolle. Sjekk om rollen er i bruk.');
    }
  };

  const handleActivateRole = async (roleId: number) => {
    try {
      setError(null);
      await roleService.activateRole(roleId);
      setSuccess('Rolle aktivert');
      loadRoles();
    } catch (error) {
      console.error('Feil ved aktivering av rolle:', error);
      setError('Kunne ikke aktivere rolle.');
    }
  };

  const handleDeactivateRole = async (roleId: number) => {
    if (!window.confirm('Er du sikker på at du vil deaktivere denne rollen?')) {
      return;
    }

    try {
      setError(null);
      await roleService.deactivateRole(roleId);
      setSuccess('Rolle deaktivert');
      loadRoles();
    } catch (error) {
      console.error('Feil ved deaktivering av rolle:', error);
      setError('Kunne ikke deaktivere rolle.');
    }
  };

  const handleFormSubmit = async (data: CreateRole | UpdateRole) => {
    try {
      setFormLoading(true);
      setError(null);

      if (viewMode === 'create') {
        await roleService.createRole(data as CreateRole);
        setSuccess('Rolle opprettet');
      } else if (viewMode === 'edit' && selectedRole) {
        await roleService.updateRole(selectedRole.id, data as UpdateRole);
        setSuccess('Rolle oppdatert');
      }

      setViewMode('list');
      setSelectedRole(null);
      loadRoles();
    } catch (error) {
      console.error('Feil ved lagring av rolle:', error);
      setError('Kunne ikke lagre rolle. Sjekk at alle påkrevde felter er fylt ut korrekt.');
    } finally {
      setFormLoading(false);
    }
  };

  const handleFormCancel = () => {
    setViewMode('list');
    setSelectedRole(null);
  };

  const clearMessages = () => {
    setError(null);
    setSuccess(null);
  };

  // Clear messages after 5 seconds
  useEffect(() => {
    if (error || success) {
      const timer = setTimeout(clearMessages, 5000);
      return () => clearTimeout(timer);
    }
  }, [error, success]);

  const renderSearchFilters = () => (
    <div className="search-filters">
      <div className="search-row">
        <div className="search-group">
          <input
            type="text"
            placeholder="Søk etter rolle..."
            value={searchParams.searchTerm || ''}
            onChange={(e) => handleSearch({ searchTerm: e.target.value })}
            className="search-input"
          />
        </div>

        <div className="search-group">
          <select
            value={searchParams.category ?? ''}
            onChange={(e) => handleSearch({ 
              category: e.target.value ? Number(e.target.value) : undefined 
            })}
            className="search-select"
          >
            <option value="">Alle kategorier</option>
            <option value={RoleCategory.Internal}>Intern</option>
            <option value={RoleCategory.External}>Ekstern</option>
            <option value={RoleCategory.System}>System</option>
          </select>
        </div>

        <div className="search-group">
          <select
            value={searchParams.level ?? ''}
            onChange={(e) => handleSearch({ 
              level: e.target.value ? Number(e.target.value) : undefined 
            })}
            className="search-select"
          >
            <option value="">Alle nivåer</option>
            <option value={OrganizationLevel.Individual}>Individuell</option>
            <option value={OrganizationLevel.Unit}>Enhet</option>
            <option value={OrganizationLevel.Department}>Avdeling</option>
            <option value={OrganizationLevel.Organization}>Organisasjon</option>
            <option value={OrganizationLevel.National}>Nasjonalt</option>
          </select>
        </div>

        <div className="search-group">
          <select
            value={searchParams.isActive?.toString() ?? ''}
            onChange={(e) => handleSearch({ 
              isActive: e.target.value ? e.target.value === 'true' : undefined 
            })}
            className="search-select"
          >
            <option value="">Alle statuser</option>
            <option value="true">Aktive</option>
            <option value="false">Inaktive</option>
          </select>
        </div>

        <button onClick={() => setSearchParams({
          searchTerm: '',
          category: undefined,
          level: undefined,
          isActive: true,
          page: 1,
          pageSize: 10
        })} className="btn btn-secondary">
          Nullstill
        </button>
      </div>
    </div>
  );

  if (viewMode === 'create' || viewMode === 'edit') {
    return (
      <div className="role-management">
        <RoleForm
          role={selectedRole || undefined}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
          loading={formLoading}
          isEdit={viewMode === 'edit'}
        />
      </div>
    );
  }

  if (viewMode === 'view' && selectedRole) {
    return (
      <div className="role-management">
        <div className="role-detail">
          <div className="role-detail-header">
            <h2>{selectedRole.name}</h2>
            <div className="detail-actions">
              <button onClick={() => handleEditRole(selectedRole)} className="btn btn-secondary">
                Rediger
              </button>
              <button onClick={() => setViewMode('list')} className="btn btn-primary">
                Tilbake til liste
              </button>
            </div>
          </div>

          <div className="role-detail-content">
            <div className="detail-section">
              <h3>Grunnleggende informasjon</h3>
              <div className="detail-grid">
                <div className="detail-item">
                  <label>Beskrivelse:</label>
                  <span>{selectedRole.description}</span>
                </div>
                <div className="detail-item">
                  <label>Kategori:</label>
                  <span>{roleService.getRoleCategoryLabel(selectedRole.category)}</span>
                </div>
                <div className="detail-item">
                  <label>Organisasjonsnivå:</label>
                  <span>{roleService.getOrganizationLevelLabel(selectedRole.level)}</span>
                </div>
                <div className="detail-item">
                  <label>Status:</label>
                  <span className={`status-badge ${selectedRole.isActive ? 'active' : 'inactive'}`}>
                    {selectedRole.isActive ? 'Aktiv' : 'Inaktiv'}
                  </span>
                </div>
              </div>
            </div>

            <div className="detail-section">
              <h3>Tilganger ({selectedRole.permissions?.length || 0})</h3>
              {selectedRole.permissions && selectedRole.permissions.length > 0 ? (
                <div className="permissions-list">
                  {selectedRole.permissions.map(permission => (
                    <div key={permission.id} className="permission-item">
                      <strong>{permission.resource}.{permission.action}</strong>
                      <small>{permission.description}</small>
                    </div>
                  ))}
                </div>
              ) : (
                <p>Denne rollen har ingen tilganger tildelt.</p>
              )}
            </div>

            <div className="detail-section">
              <h3>Metadata</h3>
              <div className="detail-grid">
                <div className="detail-item">
                  <label>Opprettet:</label>
                  <span>{new Date(selectedRole.createdAt).toLocaleDateString('no-NO')}</span>
                </div>
                <div className="detail-item">
                  <label>Opprettet av:</label>
                  <span>{selectedRole.createdByUserName}</span>
                </div>
                {selectedRole.updatedAt && (
                  <>
                    <div className="detail-item">
                      <label>Oppdatert:</label>
                      <span>{new Date(selectedRole.updatedAt).toLocaleDateString('no-NO')}</span>
                    </div>
                    {selectedRole.updatedByUserName && (
                      <div className="detail-item">
                        <label>Oppdatert av:</label>
                        <span>{selectedRole.updatedByUserName}</span>
                      </div>
                    )}
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="role-management">
      <div className="page-header">
        <h1>Rolleadministrasjon</h1>
        <div className="page-actions">
          <button onClick={handleCreateRole} className="btn btn-primary">
            + Ny rolle
          </button>
        </div>
      </div>

      {(error || success) && (
        <div className="messages">
          {error && (
            <div className="message error">
              <span>{error}</span>
              <button onClick={clearMessages} className="close-btn">×</button>
            </div>
          )}
          {success && (
            <div className="message success">
              <span>{success}</span>
              <button onClick={clearMessages} className="close-btn">×</button>
            </div>
          )}
        </div>
      )}

      {renderSearchFilters()}

      <RoleList
        roles={roles}
        totalCount={totalCount}
        currentPage={currentPage}
        pageSize={pageSize}
        totalPages={totalPages}
        loading={loading}
        onView={handleViewRole}
        onEdit={handleEditRole}
        onDelete={handleDeleteRole}
        onActivate={handleActivateRole}
        onDeactivate={handleDeactivateRole}
        onPageChange={handlePageChange}
      />
    </div>
  );
};