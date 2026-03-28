import DashboardHome from "./pages/DashboardHome";
import { Counter } from "./components/Counter";
import UsersPage from "./pages/UsersPage";

const AppRoutes = [
  {
    index: true,
    element: <DashboardHome />,
    pageTitle: 'Inicio',
  },
  {
    path: '/counter',
    element: <Counter />,
    pageTitle: 'Contador',
  },
  {
    path: '/users',
    element: <UsersPage />,
    pageTitle: 'Usuarios',
    adminOnly: true,
  },
];

export default AppRoutes;
