import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const navGroups = [
  {
    heading: 'Principal',
    items: [
      { to: '/', icon: 'bi-house-door', label: 'Inicio', end: true },
    ],
  },
  {
    heading: 'Investigación',
    items: [
      { to: '/publications', icon: 'bi-journals', label: 'Publicaciones', permission: 'publications.access' },
    ],
  },
  {
    heading: 'Administración',
    items: [
      { to: '/users', icon: 'bi-people', label: 'Usuarios', permission: 'users.view' },
      { to: '/roles', icon: 'bi-shield-lock', label: 'Roles y permisos', permission: 'roles.view' },
    ],
  },
  {
    heading: 'Sistema',
    items: [
      { to: '/settings', icon: 'bi-gear', label: 'Configuración' },
    ],
  },
];

export default function Sidebar({ collapsed, onToggle }) {
  const { hasPermission } = useAuth();

  // Only show groups that have at least one visible item
  const visibleGroups = navGroups
    .map(group => ({
      ...group,
      items: group.items.filter(item => !item.permission || hasPermission(item.permission)),
    }))
    .filter(group => group.items.length > 0);

  return (
    <aside className={`sidebar${collapsed ? ' sidebar--collapsed' : ''}`}>
      <div className="sidebar__brand">
        {!collapsed && (
          <span className="sidebar__brand-name">Dashboard v2</span>
        )}
        <button
          className="sidebar__toggle"
          onClick={onToggle}
          aria-label={collapsed ? 'Expandir menú' : 'Colapsar menú'}
        >
          <i className={`bi bi-${collapsed ? 'chevron-bar-right' : 'chevron-bar-left'}`}></i>
        </button>
      </div>

      <nav className="sidebar__nav">
        {visibleGroups.map((group) => (
          <div key={group.heading} className="sidebar__group">
            {!collapsed && (
              <span className="sidebar__group-heading">{group.heading}</span>
            )}
            {group.items.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className={({ isActive }) =>
                  `sidebar__link${isActive ? ' sidebar__link--active' : ''}`
                }
                title={collapsed ? item.label : undefined}
              >
                <i className={`bi ${item.icon} sidebar__link-icon`}></i>
                {!collapsed && (
                  <span className="sidebar__link-label">{item.label}</span>
                )}
              </NavLink>
            ))}
          </div>
        ))}
      </nav>
    </aside>
  );
}
