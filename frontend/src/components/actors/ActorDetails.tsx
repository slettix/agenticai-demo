import React, { useState, useEffect } from 'react';
import { Actor, ActorType, SecurityClearance, RoleAssignment, ActorNote, actorService, CreateActorNote } from '../../services/actorService.ts';

interface ActorDetailsProps {
  actor: Actor;
  onEdit: () => void;
  onBack: () => void;
  onDelete: () => void;
  onActivate: () => void;
  onDeactivate: () => void;
}

export const ActorDetails: React.FC<ActorDetailsProps> = ({
  actor,
  onEdit,
  onBack,
  onDelete,
  onActivate,
  onDeactivate
}) => {
  const [roles, setRoles] = useState<RoleAssignment[]>([]);
  const [notes, setNotes] = useState<ActorNote[]>([]);
  const [showAddNote, setShowAddNote] = useState(false);
  const [newNote, setNewNote] = useState<CreateActorNote>({
    note: '',
    category: '',
    isPrivate: false
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadRolesAndNotes();
  }, [actor.id]);

  const loadRolesAndNotes = async () => {
    setLoading(true);
    try {
      const [rolesData, notesData] = await Promise.all([
        actorService.getActorRoles(actor.id),
        actorService.getActorNotes(actor.id, true) // Include private notes for admins
      ]);
      setRoles(rolesData);
      setNotes(notesData);
    } catch (error) {
      console.error('Kunne ikke laste roller og notater:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAddNote = async () => {
    if (!newNote.note.trim()) return;

    try {
      const addedNote = await actorService.addActorNote(actor.id, newNote);
      setNotes(prev => [addedNote, ...prev]);
      setNewNote({ note: '', category: '', isPrivate: false });
      setShowAddNote(false);
    } catch (error) {
      console.error('Kunne ikke legge til notat:', error);
    }
  };

  const handleDeleteNote = async (noteId: number) => {
    if (!window.confirm('Er du sikker på at du vil slette dette notatet?')) return;

    try {
      await actorService.deleteActorNote(noteId);
      setNotes(prev => prev.filter(note => note.id !== noteId));
    } catch (error) {
      console.error('Kunne ikke slette notat:', error);
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

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('no-NO');
  };

  const isExternal = actor.actorType !== ActorType.Internal;

  return (
    <div className="actor-details">
      <div className="actor-details-header">
        <div className="header-info">
          <h1>{actor.fullName}</h1>
          <div className="header-meta">
            <span className={`actor-type type-${actor.actorType}`}>
              {getActorTypeLabel(actor.actorType)}
            </span>
            <span className={`status-badge ${actor.isActive ? 'active' : 'inactive'}`}>
              {actor.isActive ? 'Aktiv' : 'Inaktiv'}
            </span>
          </div>
        </div>
        
        <div className="header-actions">
          <button onClick={onEdit} className="btn btn-primary">
            Rediger
          </button>
          {actor.isActive ? (
            <button onClick={onDeactivate} className="btn btn-warning">
              Deaktiver
            </button>
          ) : (
            <button onClick={onActivate} className="btn btn-success">
              Aktiver
            </button>
          )}
          <button onClick={onDelete} className="btn btn-danger">
            Slett
          </button>
          <button onClick={onBack} className="btn btn-secondary">
            Tilbake
          </button>
        </div>
      </div>

      <div className="actor-details-content">
        <div className="details-grid">
          <div className="details-section">
            <h3>Grunnleggende informasjon</h3>
            <div className="info-grid">
              <div className="info-item">
                <label>E-post:</label>
                <span>{actor.email}</span>
              </div>
              {actor.phone && (
                <div className="info-item">
                  <label>Telefon:</label>
                  <span>{actor.phone}</span>
                </div>
              )}
              <div className="info-item">
                <label>Sikkerhetsnivå:</label>
                <span className={`security-clearance clearance-${actor.securityClearance}`}>
                  {getSecurityClearanceLabel(actor.securityClearance)}
                </span>
              </div>
              <div className="info-item">
                <label>Foretrukket språk:</label>
                <span>{actor.preferredLanguage || 'NO'}</span>
              </div>
              <div className="info-item">
                <label>Opprettet:</label>
                <span>{formatDate(actor.createdAt)} av {actor.createdByUserName}</span>
              </div>
              {actor.updatedAt && (
                <div className="info-item">
                  <label>Sist oppdatert:</label>
                  <span>{formatDate(actor.updatedAt)} av {actor.updatedByUserName}</span>
                </div>
              )}
            </div>
          </div>

          <div className="details-section">
            <h3>Organisasjon</h3>
            <div className="info-grid">
              {actor.organizationName && (
                <div className="info-item">
                  <label>Organisasjon:</label>
                  <span>{actor.organizationName}</span>
                </div>
              )}
              {actor.department && (
                <div className="info-item">
                  <label>Avdeling:</label>
                  <span>{actor.department}</span>
                </div>
              )}
              {actor.position && (
                <div className="info-item">
                  <label>Stilling:</label>
                  <span>{actor.position}</span>
                </div>
              )}
              {actor.geographicLocation && (
                <div className="info-item">
                  <label>Lokasjon:</label>
                  <span>{actor.geographicLocation}</span>
                </div>
              )}
              {actor.managerName && (
                <div className="info-item">
                  <label>Leder:</label>
                  <span>
                    {actor.managerName}
                    {actor.managerEmail && <> ({actor.managerEmail})</>}
                  </span>
                </div>
              )}
            </div>

            {actor.address && (
              <div className="info-item full-width">
                <label>Adresse:</label>
                <div className="address">{actor.address}</div>
              </div>
            )}
          </div>

          {isExternal && (
            <div className="details-section">
              <h3>Kontraktinformasjon</h3>
              <div className="info-grid">
                {actor.contractNumber && (
                  <div className="info-item">
                    <label>Kontraktnummer:</label>
                    <span>{actor.contractNumber}</span>
                  </div>
                )}
                {actor.vendorId && (
                  <div className="info-item">
                    <label>Leverandør-ID:</label>
                    <span>{actor.vendorId}</span>
                  </div>
                )}
                {actor.contractStartDate && (
                  <div className="info-item">
                    <label>Kontraktstart:</label>
                    <span>{formatDate(actor.contractStartDate)}</span>
                  </div>
                )}
                {actor.contractEndDate && (
                  <div className="info-item">
                    <label>Kontraktslutt:</label>
                    <span>{formatDate(actor.contractEndDate)}</span>
                  </div>
                )}
              </div>
            </div>
          )}

          {actor.competenceAreas && actor.competenceAreas.length > 0 && (
            <div className="details-section">
              <h3>Kompetanseområder</h3>
              <div className="tags">
                {actor.competenceAreas.map(area => (
                  <span key={area} className="tag competence-tag">{area}</span>
                ))}
              </div>
            </div>
          )}

          {actor.technicalSkills && actor.technicalSkills.length > 0 && (
            <div className="details-section">
              <h3>Tekniske ferdigheter</h3>
              <div className="tags">
                {actor.technicalSkills.map(skill => (
                  <span key={skill} className="tag skill-tag">{skill}</span>
                ))}
              </div>
            </div>
          )}

          <div className="details-section full-width">
            <h3>Roller</h3>
            {loading ? (
              <div className="loading">Laster roller...</div>
            ) : roles.length === 0 ? (
              <p>Ingen roller tildelt.</p>
            ) : (
              <div className="roles-list">
                {roles.map(role => (
                  <div key={role.roleId} className={`role-item ${!role.isActive ? 'inactive' : ''}`}>
                    <div className="role-info">
                      <strong>{role.roleName}</strong>
                      {role.roleDescription && <span className="role-description">{role.roleDescription}</span>}
                    </div>
                    <div className="role-meta">
                      <div className="assigned-info">
                        Tildelt: {formatDate(role.assignedAt)} av {role.assignedByUserName}
                      </div>
                      {role.validFrom && (
                        <div className="validity">
                          Gyldig fra: {formatDate(role.validFrom)}
                          {role.validTo && ` til ${formatDate(role.validTo)}`}
                        </div>
                      )}
                      {role.notes && (
                        <div className="role-notes">Notater: {role.notes}</div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="details-section full-width">
            <div className="section-header">
              <h3>Notater</h3>
              <button 
                onClick={() => setShowAddNote(!showAddNote)} 
                className="btn btn-sm btn-primary"
              >
                {showAddNote ? 'Avbryt' : '+ Legg til notat'}
              </button>
            </div>

            {showAddNote && (
              <div className="add-note-form">
                <div className="form-group">
                  <textarea
                    value={newNote.note}
                    onChange={(e) => setNewNote(prev => ({ ...prev, note: e.target.value }))}
                    placeholder="Skriv notat..."
                    rows={3}
                    className="form-textarea"
                  />
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <input
                      type="text"
                      value={newNote.category}
                      onChange={(e) => setNewNote(prev => ({ ...prev, category: e.target.value }))}
                      placeholder="Kategori (valgfritt)"
                      className="form-input"
                    />
                  </div>
                  <div className="form-group">
                    <label className="checkbox-label">
                      <input
                        type="checkbox"
                        checked={newNote.isPrivate}
                        onChange={(e) => setNewNote(prev => ({ ...prev, isPrivate: e.target.checked }))}
                      />
                      Privat notat
                    </label>
                  </div>
                </div>
                <div className="form-actions">
                  <button onClick={handleAddNote} className="btn btn-primary">
                    Lagre notat
                  </button>
                </div>
              </div>
            )}

            {loading ? (
              <div className="loading">Laster notater...</div>
            ) : notes.length === 0 ? (
              <p>Ingen notater registrert.</p>
            ) : (
              <div className="notes-list">
                {notes.map(note => (
                  <div key={note.id} className={`note-item ${note.isPrivate ? 'private' : ''}`}>
                    <div className="note-header">
                      <div className="note-meta">
                        {note.category && <span className="note-category">{note.category}</span>}
                        <span className="note-author">{note.createdByUserName}</span>
                        <span className="note-date">{formatDate(note.createdAt)}</span>
                        {note.isPrivate && <span className="private-badge">Privat</span>}
                      </div>
                      <button 
                        onClick={() => handleDeleteNote(note.id)}
                        className="btn btn-sm btn-danger"
                      >
                        Slett
                      </button>
                    </div>
                    <div className="note-content">{note.note}</div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};