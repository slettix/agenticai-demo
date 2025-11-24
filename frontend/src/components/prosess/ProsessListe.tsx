import React, { useState, useEffect } from 'react';
import { ProsessListItem, ProsessSearchRequest, PagedResult, ProsessStatus } from '../../types/prosess.ts';
import { prosessService } from '../../services/prosessService.ts';
import { useAuth } from '../../contexts/AuthContext.tsx';

interface ProsessListeProps {
  onProsessClick: (prosessId: number) => void;
  onCreateProsess?: () => void;
}

export const ProsessListe: React.FC<ProsessListeProps> = ({ onProsessClick, onCreateProsess }) => {
  const { hasPermission } = useAuth();
  const [prosesses, setProsesses] = useState<PagedResult<ProsessListItem>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [searchRequest, setSearchRequest] = useState<ProsessSearchRequest>({
    page: 1,
    pageSize: 20,
    sortBy: 'UpdatedAt',
    sortDescending: true
  });
  const [categories, setCategories] = useState<string[]>([]);

  useEffect(() => {
    loadProsesses();
    loadCategories();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchRequest]);

  const loadProsesses = async () => {
    try {
      setLoading(true);
      const result = await prosessService.searchProsesses(searchRequest);
      setProsesses(result);
      setError('');
    } catch (err) {
      setError('Kunne ikke laste prosesser');
      console.error('Error loading processes:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const categoriesData = await prosessService.getCategories();
      setCategories(categoriesData.businessCategories || []);
    } catch (err) {
      console.error('Error loading categories:', err);
    }
  };

  const handleSearch = (search: string) => {
    setSearchRequest(prev => ({
      ...prev,
      search,
      page: 1
    }));
  };

  const handleCategoryFilter = (category: string) => {
    setSearchRequest(prev => ({
      ...prev,
      category: category === '' ? undefined : category,
      page: 1
    }));
  };

  const handleStatusFilter = (status: string) => {
    setSearchRequest(prev => ({
      ...prev,
      status: status === '' ? undefined : parseInt(status) as ProsessStatus,
      page: 1
    }));
  };

  const handleSort = (sortBy: string) => {
    setSearchRequest(prev => ({
      ...prev,
      sortBy,
      sortDescending: prev.sortBy === sortBy ? !prev.sortDescending : true
    }));
  };

  const handlePageChange = (page: number) => {
    setSearchRequest(prev => ({ ...prev, page }));
  };

  const getStatusText = (status: ProsessStatus): string => {
    switch (status) {
      case ProsessStatus.Draft: return 'Utkast';
      case ProsessStatus.InReview: return 'Under review';
      case ProsessStatus.Approved: return 'Godkjent';
      case ProsessStatus.Published: return 'Publisert';
      case ProsessStatus.Deprecated: return 'Utdatert';
      case ProsessStatus.Archived: return 'Arkivert';
      default: return 'Ukjent';
    }
  };

  const getStatusColor = (status: ProsessStatus): string => {
    switch (status) {
      case ProsessStatus.Draft: return '#6c757d';
      case ProsessStatus.InReview: return '#ffc107';
      case ProsessStatus.Approved: return '#28a745';
      case ProsessStatus.Published: return '#007bff';
      case ProsessStatus.Deprecated: return '#fd7e14';
      case ProsessStatus.Archived: return '#dc3545';
      default: return '#6c757d';
    }
  };

  if (loading && prosesses.items.length === 0) {
    return <div className="loading-spinner">Laster prosesser...</div>;
  }

  return (
    <div className="prosess-liste">
      <div className="prosess-liste-header">
        <h2>📋 Prosessportal</h2>
        {hasPermission('create_prosess') && onCreateProsess && (
          <button
            onClick={onCreateProsess}
            className="btn-create-prosess"
            title="Opprett ny prosess"
          >
            ➕ Ny prosess
          </button>
        )}
      </div>
      
      <div className="search-filters">
        <div className="search-bar">
          <input
            type="text"
            placeholder="Søk i prosesser..."
            onChange={(e) => handleSearch(e.target.value)}
            className="search-input"
          />
        </div>
        
        <div className="filters">
          <select 
            onChange={(e) => handleCategoryFilter(e.target.value)}
            className="filter-select"
          >
            <option value="">Alle kategorier</option>
            {categories.map(cat => (
              <option key={cat} value={cat}>{cat}</option>
            ))}
          </select>
          
          <select 
            onChange={(e) => handleStatusFilter(e.target.value)}
            className="filter-select"
          >
            <option value="">Alle statuser</option>
            <option value={ProsessStatus.Draft}>Utkast</option>
            <option value={ProsessStatus.InReview}>Under review</option>
            <option value={ProsessStatus.Approved}>Godkjent</option>
            <option value={ProsessStatus.Published}>Publisert</option>
            <option value={ProsessStatus.Deprecated}>Utdatert</option>
            <option value={ProsessStatus.Archived}>Arkivert</option>
          </select>
        </div>
        
        <div className="sort-options">
          <label>Sorter etter:</label>
          <button 
            onClick={() => handleSort('Title')}
            className={`sort-btn ${searchRequest.sortBy === 'Title' ? 'active' : ''}`}
          >
            Tittel {searchRequest.sortBy === 'Title' && (searchRequest.sortDescending ? '↓' : '↑')}
          </button>
          <button 
            onClick={() => handleSort('UpdatedAt')}
            className={`sort-btn ${searchRequest.sortBy === 'UpdatedAt' ? 'active' : ''}`}
          >
            Oppdatert {searchRequest.sortBy === 'UpdatedAt' && (searchRequest.sortDescending ? '↓' : '↑')}
          </button>
          <button 
            onClick={() => handleSort('ViewCount')}
            className={`sort-btn ${searchRequest.sortBy === 'ViewCount' ? 'active' : ''}`}
          >
            Visninger {searchRequest.sortBy === 'ViewCount' && (searchRequest.sortDescending ? '↓' : '↑')}
          </button>
        </div>
      </div>

      {error && (
        <div className="error-message">
          {error}
        </div>
      )}

      {prosesses.items.length === 0 && !loading ? (
        <div className="no-results">
          <h3>Ingen prosesser funnet</h3>
          <p>Prøv å endre søkekriteriene eller 
            {hasPermission('create_prosess') && (
              <span> <button className="link-button">opprett en ny prosess</button></span>
            )}
          </p>
        </div>
      ) : (
        <>
          <div className="prosess-grid">
            {prosesses.items.map(prosess => (
              <div 
                key={prosess.id} 
                className="prosess-card"
                onClick={() => onProsessClick(prosess.id)}
              >
                <div className="card-header">
                  <h3 className="prosess-title">{prosess.title}</h3>
                  <div 
                    className="status-badge"
                    style={{ backgroundColor: getStatusColor(prosess.status) }}
                  >
                    {getStatusText(prosess.status)}
                  </div>
                </div>
                
                <p className="prosess-description">{prosess.description}</p>
                
                <div className="prosess-meta">
                  <div className="category">{prosess.category}</div>
                  <div className="view-count">👁 {prosess.viewCount} visninger</div>
                </div>
                
                <div className="tags">
                  {prosess.tags.map(tag => (
                    <span key={tag} className="tag">{tag}</span>
                  ))}
                </div>
                
                <div className="card-footer">
                  <div className="updated-info">
                    <small>
                      Oppdatert {new Date(prosess.updatedAt).toLocaleDateString('nb-NO')} 
                      av {prosess.ownerName || prosess.createdByUserName}
                    </small>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {loading && (
            <div className="loading-overlay">Laster...</div>
          )}

          {prosesses.totalPages > 1 && (
            <div className="pagination">
              <button 
                onClick={() => handlePageChange(searchRequest.page! - 1)}
                disabled={searchRequest.page === 1}
                className="page-btn"
              >
                « Forrige
              </button>
              
              <span className="page-info">
                Side {prosesses.page} av {prosesses.totalPages} 
                ({prosesses.totalCount} prosesser totalt)
              </span>
              
              <button 
                onClick={() => handlePageChange(searchRequest.page! + 1)}
                disabled={searchRequest.page === prosesses.totalPages}
                className="page-btn"
              >
                Neste »
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};