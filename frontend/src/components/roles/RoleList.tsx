import React from 'react';
import { Role, RoleCategory, OrganizationLevel } from '../../types/role.ts';

interface RoleListProps {
  roles: Role[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  loading: boolean;
  onView: (role: Role) => void;
  onEdit: (role: Role) => void;
  onDelete: (id: number) => void;
  onActivate: (id: number) => void;
  onDeactivate: (id: number) => void;
  onPageChange: (page: number) => void;
}

export const RoleList: React.FC<RoleListProps> = ({
  roles,
  totalCount,
  currentPage,
  pageSize,
  totalPages,
  loading,
  onView,
  onEdit,
  onDelete,
  onActivate,
  onDeactivate,
  onPageChange
}) => {
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

  const getRoleCategoryClass = (category: RoleCategory): string => {
    switch (category) {
      case RoleCategory.Internal: return 'category-internal';
      case RoleCategory.External: return 'category-external';
      case RoleCategory.System: return 'category-system';
      default: return 'category-unknown';
    }
  };

  const getOrganizationLevelClass = (level: OrganizationLevel): string => {
    switch (level) {
      case OrganizationLevel.Individual: return 'level-individual';
      case OrganizationLevel.Unit: return 'level-unit';
      case OrganizationLevel.Department: return 'level-department';
      case OrganizationLevel.Organization: return 'level-organization';
      case OrganizationLevel.National: return 'level-national';
      default: return 'level-unknown';
    }
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('no-NO');
  };

  const renderPagination = () => {
    if (totalPages <= 1) return null;

    const pages = [];
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    // First page
    if (startPage > 1) {
      pages.push(
        <button key={1} onClick={() => onPageChange(1)} className={`page-btn ${currentPage === 1 ? 'active' : ''}`}>
          1
        </button>
      );
      if (startPage > 2) {
        pages.push(<span key="ellipsis1" className="page-ellipsis">...</span>);
      }
    }

    // Pages around current page
    for (let i = startPage; i <= endPage; i++) {
      pages.push(
        <button
          key={i}
          onClick={() => onPageChange(i)}
          className={`page-btn ${currentPage === i ? 'active' : ''}`}
        >
          {i}
        </button>
      );
    }

    // Last page
    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        pages.push(<span key="ellipsis2" className="page-ellipsis">...</span>);
      }
      pages.push(
        <button
          key={totalPages}
          onClick={() => onPageChange(totalPages)}
          className={`page-btn ${currentPage === totalPages ? 'active' : ''}`}
        >
          {totalPages}
        </button>
      );
    }

    return (
      <div className="pagination">
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage <= 1}
          className="page-btn prev"
        >
          ← Forrige
        </button>
        {pages}
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage >= totalPages}
          className="page-btn next"
        >
          Neste →
        </button>
      </div>
    );
  };

  if (loading) {
    return (
      <div className="role-list-loading">
        <div className="loading-spinner"></div>
        <p>Laster roller...</p>
      </div>
    );
  }

  return (
    <div className="role-list">
      <div className="role-list-header">
        <div className="result-info">
          <span className="result-count">
            Viser {Math.min((currentPage - 1) * pageSize + 1, totalCount)} - {Math.min(currentPage * pageSize, totalCount)} av {totalCount} roller
          </span>
        </div>
      </div>

      {roles.length === 0 ? (
        <div className="no-roles">
          <p>Ingen roller funnet med de valgte søkekriteriene.</p>
        </div>
      ) : (
        <>
          <div className="roles-table">
            <div className="table-header">
              <div className="col-name">Navn</div>
              <div className="col-description">Beskrivelse</div>
              <div className="col-category">Kategori</div>
              <div className="col-level">Organisasjonsnivå</div>
              <div className="col-permissions">Tilganger</div>
              <div className="col-created">Opprettet</div>
              <div className="col-status">Status</div>
              <div className="col-actions">Handlinger</div>
            </div>

            <div className="table-body">
              {roles.map(role => (
                <div key={role.id} className={`role-row ${!role.isActive ? 'inactive' : ''}`}>
                  <div className="col-name">
                    <div className="role-name">
                      <strong>{role.name}</strong>
                    </div>
                  </div>

                  <div className="col-description">
                    <div className="description-text" title={role.description}>
                      {role.description.length > 100 
                        ? `${role.description.substring(0, 97)}...`
                        : role.description}
                    </div>
                  </div>

                  <div className="col-category">
                    <span className={`role-category ${getRoleCategoryClass(role.category)}`}>
                      {getRoleCategoryLabel(role.category)}
                    </span>
                  </div>

                  <div className="col-level">
                    <span className={`organization-level ${getOrganizationLevelClass(role.level)}`}>
                      {getOrganizationLevelLabel(role.level)}
                    </span>
                  </div>

                  <div className="col-permissions">
                    {role.permissions && role.permissions.length > 0 ? (
                      <div className="permissions-count">
                        {role.permissions.length} tilgang{role.permissions.length !== 1 ? 'er' : ''}
                      </div>
                    ) : (
                      <span className="no-permissions">Ingen tilganger</span>
                    )}
                  </div>

                  <div className="col-created">
                    <div className="creation-info">
                      <div className="created-date">{formatDate(role.createdAt)}</div>
                      <div className="created-by">{role.createdByUserName}</div>
                    </div>
                    {role.updatedAt && (
                      <div className="updated-info">
                        <div className="updated-date">Oppdatert: {formatDate(role.updatedAt)}</div>
                        {role.updatedByUserName && (
                          <div className="updated-by">{role.updatedByUserName}</div>
                        )}
                      </div>
                    )}
                  </div>

                  <div className="col-status">
                    <span className={`status-badge ${role.isActive ? 'active' : 'inactive'}`}>
                      {role.isActive ? 'Aktiv' : 'Inaktiv'}
                    </span>
                  </div>

                  <div className="col-actions">
                    <div className="action-buttons">
                      <button
                        onClick={() => onView(role)}
                        className="btn btn-sm btn-primary"
                        title="Se detaljer"
                      >
                        👁
                      </button>
                      <button
                        onClick={() => onEdit(role)}
                        className="btn btn-sm btn-secondary"
                        title="Rediger"
                      >
                        ✏️
                      </button>
                      {role.isActive ? (
                        <button
                          onClick={() => onDeactivate(role.id)}
                          className="btn btn-sm btn-warning"
                          title="Deaktiver"
                        >
                          ⏸
                        </button>
                      ) : (
                        <button
                          onClick={() => onActivate(role.id)}
                          className="btn btn-sm btn-success"
                          title="Aktiver"
                        >
                          ▶️
                        </button>
                      )}
                      <button
                        onClick={() => onDelete(role.id)}
                        className="btn btn-sm btn-danger"
                        title="Slett permanent"
                      >
                        🗑
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {renderPagination()}
        </>
      )}
    </div>
  );
};