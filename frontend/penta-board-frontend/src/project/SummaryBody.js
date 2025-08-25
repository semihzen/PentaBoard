export default function SummaryBody({ project }) {
  return (
    <div className="summary-wrap">
      <div className="card about">
        <h3>About this project</h3>
        <p className="muted">
          Describe your project and make it easier for other people to understand it.
        </p>
        <button className="btn ghost">+ Add Project Description</button>
      </div>

      <div className="right-rail">
        <div className="card stats">
          <div className="rail-head">
            <h4>Project stats</h4>
            <span className="muted small">Last 7 days</span>
          </div>
          <div className="stat-grid">
            <div className="stat">
              <div className="kpi">1</div>
              <div className="muted small">Work items created</div>
            </div>
            <div className="stat">
              <div className="kpi">0</div>
              <div className="muted small">Work items completed</div>
            </div>
          </div>
        </div>

        <div className="card members">
          <h4>Members</h4>
          <div className="member-pill">{(project.name?.[0] || 'P').toUpperCase()}</div>
        </div>
      </div>
    </div>
  );
}
