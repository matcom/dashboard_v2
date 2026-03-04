import DashboardHome from "./pages/DashboardHome";
import { Counter } from "./components/Counter";

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
];

export default AppRoutes;
