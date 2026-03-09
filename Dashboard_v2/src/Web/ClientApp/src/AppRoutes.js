import DashboardHome from "./pages/DashboardHome";
import PublicationsPage from "./pages/PublicationsPage";
import RolesPage from "./pages/RolesPage";
import UsersPage from "./pages/UsersPage";
import { Counter } from "./components/Counter";

const AppRoutes = [
  {
    index: true,
    element: <DashboardHome />,
    pageTitle: 'Inicio',
  },
  {
    path: '/publications',
    element: <PublicationsPage />,
    pageTitle: 'Publicaciones',
    permission: 'publications.access',
  },
  {
    path: '/users',
    element: <UsersPage />,
    pageTitle: 'Usuarios',
    permission: 'users.view',
  },
  {
    path: '/roles',
    element: <RolesPage />,
    pageTitle: 'Roles y permisos',
    permission: 'roles.view',
  },
  {
    path: '/counter',
    element: <Counter />,
    pageTitle: 'Contador',
  },
];

export default AppRoutes;
