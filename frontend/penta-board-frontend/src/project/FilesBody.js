export default function FilesBody({ project }) {
  return (
    <div className="empty-state">
      <div className="empty-icon">📁</div>
      <h3>Files</h3>
      <p>Project files & attachments for <strong>{project.name}</strong> will appear here.</p>
    </div>
  );
}
