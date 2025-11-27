import React, { useState, useEffect } from 'react';
import { ActorList } from './ActorList.tsx';
import { ActorForm } from './ActorForm.tsx';
import { ActorSearch, ActorCategory, ActorType, SecurityClearance } from './ActorSearch.tsx';
import { ActorDetails } from './ActorDetails.tsx';
import { actorService, Actor, ActorSearch as ActorSearchType, CreateActor, UpdateActor } from '../../services/actorService.ts';
import './actors.css';

interface ActorManagementState {
  view: 'list' | 'create' | 'edit' | 'details';
  selectedActor?: Actor;
  searchCriteria: ActorSearchType;
  loading: boolean;
  error: string | null;
}

export const ActorManagement: React.FC = () => {
  const [state, setState] = useState<ActorManagementState>({
    view: 'list',
    searchCriteria: {
      page: 1,
      pageSize: 20,
      isActive: true
    },
    loading: false,
    error: null
  });

  const [actors, setActors] = useState<Actor[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  // Load actors when search criteria changes
  useEffect(() => {
    loadActors();
  }, [state.searchCriteria]);

  const loadActors = async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const result = await actorService.getActors(state.searchCriteria);
      setActors(result.actors);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (error) {
      setState(prev => ({ ...prev, error: `Kunne ikke laste aktører: ${error}` }));
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  };

  const handleSearch = (searchCriteria: ActorSearchType) => {
    setState(prev => ({
      ...prev,
      searchCriteria: { ...searchCriteria, page: 1 }, // Reset to first page
      view: 'list'
    }));
  };

  const handleCreateActor = async (createActor: CreateActor) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      await actorService.createActor(createActor);
      await loadActors();
      setState(prev => ({ ...prev, view: 'list' }));
    } catch (error) {
      setState(prev => ({ ...prev, error: `Kunne ikke opprette aktør: ${error}` }));
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  };

  const handleUpdateActor = async (id: number, updateActor: UpdateActor) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const updatedActor = await actorService.updateActor(id, updateActor);
      setActors(prev => prev.map(actor => 
        actor.id === id ? updatedActor : actor
      ));
      setState(prev => ({ ...prev, view: 'list', selectedActor: undefined }));
    } catch (error) {
      setState(prev => ({ ...prev, error: `Kunne ikke oppdatere aktør: ${error}` }));
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  };

  const handleDeleteActor = async (id: number) => {
    if (!window.confirm('Er du sikker på at du vil slette denne aktøren?')) {
      return;
    }

    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      await actorService.deleteActor(id);
      await loadActors();
    } catch (error) {
      setState(prev => ({ ...prev, error: `Kunne ikke slette aktør: ${error}` }));
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  };

  const handleActivateActor = async (id: number) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      await actorService.activateActor(id);
      await loadActors();
    } catch (error) {
      setState(prev => ({ ...prev, error: `Kunne ikke aktivere aktør: ${error}` }));
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  };

  const handleDeactivateActor = async (id: number) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      await actorService.deactivateActor(id);
      await loadActors();
    } catch (error) {
      setState(prev => ({ ...prev, error: `Kunne ikke deaktivere aktør: ${error}` }));
    } finally {
      setState(prev => ({ ...prev, loading: false }));
    }
  };

  const handleViewActor = (actor: Actor) => {
    setState(prev => ({ ...prev, view: 'details', selectedActor: actor }));
  };

  const handleEditActor = (actor: Actor) => {
    setState(prev => ({ ...prev, view: 'edit', selectedActor: actor }));
  };

  const handlePageChange = (page: number) => {
    setState(prev => ({
      ...prev,
      searchCriteria: { ...prev.searchCriteria, page }
    }));
  };

  const handleBackToList = () => {
    setState(prev => ({ ...prev, view: 'list', selectedActor: undefined }));
  };

  const renderContent = () => {
    switch (state.view) {
      case 'create':
        return (
          <ActorForm
            onSubmit={handleCreateActor}
            onCancel={handleBackToList}
            loading={state.loading}
          />
        );

      case 'edit':
        return state.selectedActor ? (
          <ActorForm
            actor={state.selectedActor}
            onSubmit={(updateData) => handleUpdateActor(state.selectedActor!.id, updateData)}
            onCancel={handleBackToList}
            loading={state.loading}
            isEdit
          />
        ) : null;

      case 'details':
        return state.selectedActor ? (
          <ActorDetails
            actor={state.selectedActor}
            onEdit={() => handleEditActor(state.selectedActor!)}
            onBack={handleBackToList}
            onDelete={() => handleDeleteActor(state.selectedActor!.id)}
            onActivate={() => handleActivateActor(state.selectedActor!.id)}
            onDeactivate={() => handleDeactivateActor(state.selectedActor!.id)}
          />
        ) : null;

      default:
        return (
          <>
            <ActorSearch
              onSearch={handleSearch}
              loading={state.loading}
              currentCriteria={state.searchCriteria}
            />
            
            <ActorList
              actors={actors}
              totalCount={totalCount}
              currentPage={state.searchCriteria.page || 1}
              pageSize={state.searchCriteria.pageSize || 20}
              totalPages={totalPages}
              loading={state.loading}
              onView={handleViewActor}
              onEdit={handleEditActor}
              onDelete={handleDeleteActor}
              onActivate={handleActivateActor}
              onDeactivate={handleDeactivateActor}
              onPageChange={handlePageChange}
            />
          </>
        );
    }
  };

  return (
    <div className="actor-management">
      <div className="actor-management-header">
        <div className="header-content">
          <h1>Aktører og Roller</h1>
          <div className="header-actions">
            {state.view === 'list' && (
              <button 
                className="btn btn-primary"
                onClick={() => setState(prev => ({ ...prev, view: 'create' }))}
                disabled={state.loading}
              >
                + Ny Aktør
              </button>
            )}
            
            {state.view !== 'list' && (
              <button 
                className="btn btn-secondary"
                onClick={handleBackToList}
              >
                ← Tilbake til liste
              </button>
            )}
          </div>
        </div>
        
        {state.error && (
          <div className="error-message">
            {state.error}
            <button 
              className="close-btn"
              onClick={() => setState(prev => ({ ...prev, error: null }))}
            >
              ×
            </button>
          </div>
        )}
      </div>

      <div className="actor-management-content">
        {renderContent()}
      </div>
    </div>
  );
};