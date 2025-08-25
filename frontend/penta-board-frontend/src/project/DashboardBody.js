export default function DashboardBody({ project }) {
  return (
    <div className="empty-state">
      <div className="empty-icon">📊</div>
      <h3>Dashboard</h3>
      <p>Widgets and reports for <strong>{project.name}</strong> will live here.</p>
    </div>
  );
}
