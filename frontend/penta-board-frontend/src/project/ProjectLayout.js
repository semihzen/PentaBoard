import SummaryBody from './SummaryBody';
import DashboardBody from './DashboardBody';
import FilesBody from './FilesBody';

export default function ProjectLayout({ slug, project, sub, navigate }) {
  const pk = project.key || project.id;

  const render = () => {
    switch (sub) {
      case 'dashboard':
        return <DashboardBody project={project} />;
      case 'files':
        return <FilesBody project={project} />;
      case 'summary':
      default:
        return <SummaryBody project={project} />;
    }
  };

  return (
    <div className="project-body">
      {render()}
    </div>
  );
}
