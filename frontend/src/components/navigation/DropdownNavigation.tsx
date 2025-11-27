import React, { useState, useRef, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext.tsx';
import './navigation.css';

interface DropdownItem {
  path: string;
  label: string;
  icon: string;
  requiredPermission?: string;
  requiredRole?: string;
}

interface DropdownGroup {
  id: string;
  label: string;
  icon: string;
  items: DropdownItem[];
  requiredPermission?: string;
  requiredRole?: string;
}

const DropdownNavigation: React.FC = () => {
  const { user, logout, hasPermission, hasRole } = useAuth();
  const location = useLocation();
  const [activeDropdown, setActiveDropdown] = useState<string | null>(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const dropdownTimeouts = useRef<{ [key: string]: NodeJS.Timeout }>({});

  // Navigation structure
  const navigationGroups: DropdownGroup[] = [
    {
      id: 'process-admin',
      label: 'Prosessadministrasjon',
      icon: '📋',
      items: [
        { path: '/prosessadministrasjon/godkjenning', label: 'Til kvalitetssikring', icon: '✅', requiredPermission: 'view_qa_queue' },
        { path: '/prosessadministrasjon/opprett', label: 'Opprett ny prosess', icon: '➕', requiredPermission: 'create_prosess' },
        { path: '/prosessadministrasjon/mine', label: 'Mine prosedyrer', icon: '📝', requiredRole: 'ProsessEier' },
        { path: '/prosessadministrasjon/arkiverte', label: 'Arkiverte prosedyrer', icon: '🗑️', requiredPermission: 'delete_prosess' }
      ]
    },
    {
      id: 'user-admin',
      label: 'Bruker- og Tilgangsadministrasjon', 
      icon: '👥',
      items: [
        { path: '/brukeradministrasjon/aktorer', label: 'Aktører', icon: '👤', requiredRole: 'Admin' },
        { path: '/brukeradministrasjon/roller', label: 'Roller og Tilganger', icon: '🛡️', requiredPermission: 'manage_roles' }
      ]
    },
    {
      id: 'system-admin',
      label: 'Systemadministrasjon',
      icon: '⚙️',
      requiredRole: 'Admin',
      items: [
        { path: '/systemadministrasjon/innstillinger', label: 'Systeminnstillinger', icon: '🔧', requiredRole: 'Admin' },
        { path: '/systemadministrasjon/rapporter', label: 'Systemrapporter', icon: '📊', requiredRole: 'Admin' },
        { path: '/systemadministrasjon/audit', label: 'Audit Logs', icon: '🔍', requiredRole: 'Admin' }
      ]
    }
  ];

  // Check if user has access to any item in a group
  const hasGroupAccess = (group: DropdownGroup): boolean => {
    if (group.requiredRole && !hasRole(group.requiredRole)) return false;
    if (group.requiredPermission && !hasPermission(group.requiredPermission)) return false;
    
    return group.items.some(item => {
      if (item.requiredRole && !hasRole(item.requiredRole)) return false;
      if (item.requiredPermission && !hasPermission(item.requiredPermission)) return false;
      return true;
    });
  };

  // Check if user has access to specific item
  const hasItemAccess = (item: DropdownItem): boolean => {
    if (item.requiredRole && !hasRole(item.requiredRole)) return false;
    if (item.requiredPermission && !hasPermission(item.requiredPermission)) return false;
    return true;
  };

  // Check if current path is within a group
  const isGroupActive = (group: DropdownGroup): boolean => {
    return group.items.some(item => location.pathname.startsWith(item.path));
  };

  // Handle dropdown mouse events
  const handleMouseEnter = (groupId: string) => {
    if (dropdownTimeouts.current[groupId]) {
      clearTimeout(dropdownTimeouts.current[groupId]);
    }
    setActiveDropdown(groupId);
  };

  const handleMouseLeave = (groupId: string) => {
    dropdownTimeouts.current[groupId] = setTimeout(() => {
      setActiveDropdown(null);
    }, 150);
  };

  // Handle mobile menu
  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  const closeMobileMenu = () => {
    setIsMobileMenuOpen(false);
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = () => {
      setActiveDropdown(null);
      setIsMobileMenuOpen(false);
    };

    document.addEventListener('click', handleClickOutside);
    return () => document.removeEventListener('click', handleClickOutside);
  }, []);

  // Close mobile menu when route changes
  useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [location.pathname]);

  // Return null if no user after all hooks have been called
  if (!user) return null;

  const accessibleGroups = navigationGroups.filter(hasGroupAccess);

  return (
    <>
      {/* Desktop Navigation */}
      <nav className="dropdown-nav desktop-nav">
        <div className="nav-content">
          {/* Home/Logo link */}
          <Link to="/prosesser" className="nav-home">
            🏠 Prosessoversikt
          </Link>

          {/* Dropdown groups */}
          <div className="nav-groups">
            {accessibleGroups.map(group => (
              <div
                key={group.id}
                className={`nav-group ${isGroupActive(group) ? 'active' : ''}`}
                onMouseEnter={() => handleMouseEnter(group.id)}
                onMouseLeave={() => handleMouseLeave(group.id)}
                onClick={(e) => e.stopPropagation()}
              >
                <button className="group-trigger">
                  <span className="group-icon">{group.icon}</span>
                  <span className="group-label">{group.label}</span>
                  <span className="dropdown-arrow">▼</span>
                </button>

                {activeDropdown === group.id && (
                  <div className="dropdown-menu">
                    {group.items.filter(hasItemAccess).map(item => (
                      <Link
                        key={item.path}
                        to={item.path}
                        className={`dropdown-item ${location.pathname === item.path ? 'active' : ''}`}
                      >
                        <span className="item-icon">{item.icon}</span>
                        <span className="item-label">{item.label}</span>
                      </Link>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* User menu */}
          <div className="user-menu">
            <div className="user-info-compact">
              <span className="user-name">{user.firstName} {user.lastName}</span>
              <button onClick={logout} className="logout-btn">
                Logg ut
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Mobile Navigation */}
      <nav className="dropdown-nav mobile-nav">
        <div className="mobile-nav-header">
          <Link to="/prosesser" className="mobile-home" onClick={closeMobileMenu}>
            🏠 Prosessoversikt
          </Link>
          <button className="mobile-menu-toggle" onClick={toggleMobileMenu}>
            {isMobileMenuOpen ? '✕' : '☰'}
          </button>
        </div>

        {isMobileMenuOpen && (
          <div className="mobile-menu">
            <div className="mobile-menu-content">
              {accessibleGroups.map(group => (
                <div key={group.id} className="mobile-group">
                  <div className="mobile-group-header">
                    <span className="group-icon">{group.icon}</span>
                    <span className="group-label">{group.label}</span>
                  </div>
                  <div className="mobile-group-items">
                    {group.items.filter(hasItemAccess).map(item => (
                      <Link
                        key={item.path}
                        to={item.path}
                        className={`mobile-item ${location.pathname === item.path ? 'active' : ''}`}
                        onClick={closeMobileMenu}
                      >
                        <span className="item-icon">{item.icon}</span>
                        <span className="item-label">{item.label}</span>
                      </Link>
                    ))}
                  </div>
                </div>
              ))}

              <div className="mobile-user-section">
                <div className="mobile-user-info">
                  <span className="user-name">{user.firstName} {user.lastName}</span>
                  <div className="user-roles">
                    {user.roles.map(role => (
                      <span key={role} className="role-badge">{role}</span>
                    ))}
                  </div>
                </div>
                <button onClick={logout} className="mobile-logout">
                  Logg ut
                </button>
              </div>
            </div>
          </div>
        )}
      </nav>

      {/* Mobile menu overlay */}
      {isMobileMenuOpen && <div className="mobile-menu-overlay" onClick={closeMobileMenu} />}
    </>
  );
};

export default DropdownNavigation;