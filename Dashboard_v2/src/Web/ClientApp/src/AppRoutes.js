import DashboardHome from "./pages/DashboardHome";
import PublicationsPage from "./pages/PublicationsPage";
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
  },
  {
    path: '/counter',
    element: <Counter />,
    pageTitle: 'Contador',
  },
];

export default AppRoutes;
