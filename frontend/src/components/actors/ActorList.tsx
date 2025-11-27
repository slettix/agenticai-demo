import React from 'react';
import { Actor, ActorCategory, ActorType, SecurityClearance } from '../../services/actorService.ts';

interface ActorListProps {
  actors: Actor[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  loading: boolean;
  onView: (actor: Actor) => void;
  onEdit: (actor: Actor) => void;
  onDelete: (id: number) => void;
  onActivate: (id: number) => void;
  onDeactivate: (id: number) => void;
  onPageChange: (page: number) => void;
}

export const ActorList: React.FC<ActorListProps> = ({
  actors,
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
  const getActorCategoryLabel = (category: ActorCategory): string => {
    switch (category) {
      case ActorCategory.Person: return 'Person';
      case ActorCategory.Organization: return 'Org';
      case ActorCategory.Unit: return 'Enhet';
      default: return 'Ukjent';
    }
  };

  const getActorTypeLabel = (type: ActorType): string => {
    switch (type) {
      case ActorType.Internal: return 'Intern';
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

  const getSecurityClearanceClass = (clearance: SecurityClearance): string => {
    switch (clearance) {
      case SecurityClearance.None: return 'clearance-none';
      case SecurityClearance.Restricted: return 'clearance-restricted';
      case SecurityClearance.Confidential: return 'clearance-confidential';
      case SecurityClearance.Secret: return 'clearance-secret';
      case SecurityClearance.TopSecret: return 'clearance-topsecret';
      default: return 'clearance-none';
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
      <div className="actor-list-loading">
        <div className="loading-spinner"></div>
        <p>Laster aktører...</p>
      </div>
    );
  }

  return (
    <div className="actor-list">
      <div className="actor-list-header">
        <div className="result-info">
          <span className="result-count">
            Viser {Math.min((currentPage - 1) * pageSize + 1, totalCount)} - {Math.min(currentPage * pageSize, totalCount)} av {totalCount} aktører
          </span>
        </div>
      </div>

      {actors.length === 0 ? (
        <div className="no-actors">
          <p>Ingen aktører funnet med de valgte søkekriteriene.</p>
        </div>
      ) : (
        <>
          <div className="actors-table">
            <div className="table-header">
              <div className="col-name">Navn</div>
              <div className="col-category">Kategori</div>
              <div className="col-organization">Organisasjon</div>
              <div className="col-type">Type</div>
              <div className="col-clearance">Sikkerhetsnivå</div>
              <div className="col-contact">Kontakt</div>
              <div className="col-status">Status</div>
              <div className="col-actions">Handlinger</div>
            </div>

            <div className="table-body">
              {actors.map(actor => (
                <div key={actor.id} className={`actor-row ${!actor.isActive ? 'inactive' : ''}`}>
                  <div className="col-name">
                    <div className="actor-name">
                      <strong>{actor.displayName}</strong>
                      {actor.position && <span className="position">({actor.position})</span>}
                    </div>
                    {actor.department && (
                      <div className="department">{actor.department}</div>
                    )}
                    {actor.actorCategory === ActorCategory.Unit && actor.unitCode && (
                      <div className="unit-code">Kode: {actor.unitCode}</div>
                    )}
                  </div>

                  <div className="col-category">
                    <span className={`actor-category category-${actor.actorCategory}`}>
                      {getActorCategoryLabel(actor.actorCategory)}
                    </span>
                  </div>

                  <div className="col-organization">
                    {actor.actorCategory === ActorCategory.Organization ? actor.organizationName : 
                     actor.actorCategory === ActorCategory.Unit ? actor.unitName :
                     actor.organizationName || '-'}
                  </div>

                  <div className="col-type">
                    <span className={`actor-type type-${actor.actorType}`}>
                      {getActorTypeLabel(actor.actorType)}
                    </span>
                  </div>

                  <div className="col-clearance">
                    <span className={`security-clearance ${getSecurityClearanceClass(actor.securityClearance)}`}>
                      {getSecurityClearanceLabel(actor.securityClearance)}
                    </span>
                  </div>

                  <div className="col-contact">
                    <div className="email">{actor.email}</div>
                    {actor.phone && <div className="phone">{actor.phone}</div>}
                  </div>

                  <div className="col-status">
                    <span className={`status-badge ${actor.isActive ? 'active' : 'inactive'}`}>
                      {actor.isActive ? 'Aktiv' : 'Inaktiv'}
                    </span>
                    {actor.assignedRoles && actor.assignedRoles.length > 0 && (
                      <div className="role-count">
                        {actor.assignedRoles.length} rolle(r)
                      </div>
                    )}
                  </div>

                  <div className="col-actions">
                    <div className="action-buttons">
                      <button
                        onClick={() => onView(actor)}
                        className="btn btn-sm btn-primary"
                        title="Se detaljer"
                      >
                        👁
                      </button>
                      <button
                        onClick={() => onEdit(actor)}
                        className="btn btn-sm btn-secondary"
                        title="Rediger"
                      >
                        ✏️
                      </button>
                      {actor.isActive ? (
                        <button
                          onClick={() => onDeactivate(actor.id)}
                          className="btn btn-sm btn-warning"
                          title="Deaktiver"
                        >
                          ⏸
                        </button>
                      ) : (
                        <button
                          onClick={() => onActivate(actor.id)}
                          className="btn btn-sm btn-success"
                          title="Aktiver"
                        >
                          ▶️
                        </button>
                      )}
                      <button
                        onClick={() => onDelete(actor.id)}
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