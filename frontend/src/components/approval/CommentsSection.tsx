import React, { useState } from 'react';
import { ProsessApprovalComment, CommentType, AddApprovalCommentRequest } from '../../types/approval.ts';
import { approvalService } from '../../services/approvalService.ts';

interface CommentsSectionProps {
  approvalRequestId: number;
  existingComments: ProsessApprovalComment[];
}

export const CommentsSection: React.FC<CommentsSectionProps> = ({
  approvalRequestId,
  existingComments
}) => {
  const [comments, setComments] = useState<ProsessApprovalComment[]>(existingComments);
  const [newComment, setNewComment] = useState('');
  const [commentType, setCommentType] = useState<CommentType>(CommentType.General);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!newComment.trim()) return;

    try {
      setLoading(true);
      setError(null);

      const request: AddApprovalCommentRequest = {
        comment: newComment.trim(),
        type: commentType
      };

      const addedComment = await approvalService.addComment(approvalRequestId, request);
      setComments([...comments, addedComment]);
      setNewComment('');
      setCommentType(CommentType.General);
    } catch (err: any) {
      console.error('Error adding comment:', err);
      setError(err.message || 'Feil ved tillegg av kommentar');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('no-NO');
  };

  const getCommentTypeIcon = (type: CommentType) => {
    switch (type) {
      case CommentType.General: return '💬';
      case CommentType.Question: return '❓';
      case CommentType.Suggestion: return '💡';
      case CommentType.Issue: return '⚠️';
      case CommentType.Approval: return '✅';
      case CommentType.Rejection: return '❌';
      default: return '💬';
    }
  };

  return (
    <div className="comments-section">
      <h4>Kommentarer</h4>
      
      {comments.length === 0 ? (
        <p className="no-comments">Ingen kommentarer ennå.</p>
      ) : (
        <div className="comments-list">
          {comments.map((comment) => (
            <div key={comment.id} className="comment">
              <div className="comment-header">
                <span className="comment-author">
                  {getCommentTypeIcon(comment.type)} {comment.userName}
                </span>
                <span className="comment-type">
                  {approvalService.getCommentTypeText(comment.type)}
                </span>
                <span className="comment-date">
                  {formatDate(comment.createdAt)}
                </span>
              </div>
              <div className="comment-content">
                {comment.comment}
              </div>
            </div>
          ))}
        </div>
      )}

      <form onSubmit={handleAddComment} className="add-comment-form">
        <div className="comment-type-selector">
          <label htmlFor="comment-type">Type:</label>
          <select
            id="comment-type"
            value={commentType}
            onChange={(e) => setCommentType(Number(e.target.value) as CommentType)}
            disabled={loading}
          >
            <option value={CommentType.General}>Generell kommentar</option>
            <option value={CommentType.Question}>Spørsmål</option>
            <option value={CommentType.Suggestion}>Forslag</option>
            <option value={CommentType.Issue}>Problem/Bekymring</option>
          </select>
        </div>

        <div className="comment-input">
          <textarea
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Skriv din kommentar..."
            rows={3}
            disabled={loading}
          />
        </div>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}

        <div className="comment-actions">
          <button
            type="submit"
            disabled={!newComment.trim() || loading}
            className="add-comment-button"
          >
            {loading ? (
              <>
                <span className="spinner-small"></span>
                Legger til...
              </>
            ) : (
              '💬 Legg til kommentar'
            )}
          </button>
        </div>
      </form>
    </div>
  );
};