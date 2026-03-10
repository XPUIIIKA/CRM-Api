import { Outlet, Link, useLocation, useNavigate } from "react-router-dom";
import logoImage from "../assets/logo.svg";
import homeImage from "../assets/layout/home.svg";
import productsImage from "../assets/layout/products.svg";
import managersImage from "../assets/layout/managers.svg";
import ordersImage from "../assets/layout/orders.svg";
import statisticsImage from "../assets/layout/statistics.svg";
import messagesImage from "../assets/layout/messages.svg";

import { useDispatch, useSelector } from "react-redux";
import { logout } from "../store/authSlice";

export default function Layout() {
  const location = useLocation();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const handleLogout = () => {
    dispatch(logout());
    navigate("/login");
  };

  const menuItems = [
    { path: "/", icon: homeImage, alt: "Дашборд" },
    { path: "/products", icon: productsImage, alt: "Товари" },
    { path: "/orders", icon: ordersImage, alt: "Замовлення" },
    { path: "/managers", icon: managersImage, alt: "Команда" },
    { path: "/statistics", icon: statisticsImage, alt: "Статистика" },
    { path: "/messages", icon: messagesImage, alt: "Повідомлення" },
  ];

  const user = useSelector((state) => state.auth.user);

  const displayRole = user?.role?.name
    ? user.role.name
    : user?.isSystemAdmin
      ? "Системний Адміністратор"
      : "Користувач";

  const userName = user?.fullName || user?.login || "User";
  const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(userName)}&background=8ba870&color=fff&size=150`;

  return (
    <div className="h-screen flex flex-col font-sans overflow-hidden">
      {/* ШАПКА */}
      <header className="h-[50px] bg-falcon-dark flex items-center justify-between px-6 shrink-0 z-20 shadow-md">
        <div className="flex items-center gap-3 text-white">
          <img
            src={logoImage}
            alt="Falcon Logo"
            className="h-10 w-auto object-contain"
          />
          <span className="text-3xl font-light tracking-wide">Falcon</span>
        </div>

        <div className="flex items-center gap-6">
          <input
            type="text"
            placeholder="Пошук"
            className="w-64 bg-falcon-light text-falcon-dark placeholder-falcon-dark/70 rounded-full px-4 py-1.5 focus:outline-none focus:ring-1 focus:ring-white transition-all"
          />
          
          <div className="flex flex-col text-right">
            <span className="text-sm font-medium text-white">
              {userName}
            </span>
            <span className="text-xs text-white">{displayRole}</span>
          </div>
          
          <Link to={"/profile"}>
            <div className="h-10 w-10 bg-gray-300 rounded-full overflow-hidden border border-white hover:border-falcon-light transition-colors shadow-sm relative group">
              <img
                src={avatarUrl}
                alt={userName}
                className="h-full w-full object-cover group-hover:scale-110 transition-transform duration-300"
              />
            </div>
          </Link>

          <button
            onClick={handleLogout}
            className="text-white hover:text-falcon-light transition-colors hover:scale-110"
            title="Вийти"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="1.5"
                d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
              />
            </svg>
          </button>
        </div>
      </header>

      <div className="flex flex-1 overflow-hidden">
        {/* САЙДБАР */}
        <aside className="w-[50px] bg-falcon-accent flex flex-col items-center py-6 shrink-0 z-10 shadow-[2px_0_10px_rgba(0,0,0,0.05)] relative">
          
          {/* Основне меню */}
          <div className="flex flex-col gap-2 w-full items-center">
            {menuItems.map((item) => {
              const isActive = location.pathname === item.path;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  title={item.alt}
                  className={`w-12 h-12 flex items-center justify-center rounded-xl transition-all relative ${
                    isActive
                      ? "bg-falcon-dark shadow-md" 
                      : "hover:bg-falcon-dark/20"
                  }`}
                >
                  {/* Декоративна крапка зліва для активного пункту */}
                  {isActive && (
                    <div className="absolute -left-1 top-1/2 -translate-y-1/2 w-2 h-4 bg-white rounded-r-md"></div>
                  )}
                  <img
                    src={item.icon}
                    alt={item.alt}
                    className={`w-6 h-6 object-contain ${isActive ? "brightness-200" : ""}`}
                  />
                </Link>
              );
            })}
          </div>

          {/* КНОПКА АДМІН-ПАНЕЛІ (В самому низу) */}
          {user?.isSystemAdmin && (
            <div className="mt-auto w-full flex flex-col items-center mb-2">
              {/* Розділювач */}
              <div className="w-6 h-[2px] bg-falcon-dark/20 rounded-full mb-3"></div>
              
              <Link
                to="/admin"
                title="Панель Супер-Адміна"
                className={`w-12 h-12 flex items-center justify-center rounded-xl transition-all group ${
                  location.pathname === "/admin"
                    ? "bg-falcon-dark text-white shadow-md"
                    : "text-falcon-dark hover:bg-falcon-dark/20"
                }`}
              >
                {/* SVG-іконка шестерні */}
                <svg 
                  className={`w-6 h-6 transition-transform duration-500 ${location.pathname !== "/admin" && "group-hover:rotate-90"}`} 
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </Link>
            </div>
          )}

        </aside>

        {/* КОНТЕНТ */}
        <main className="flex-1 bg-white overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}