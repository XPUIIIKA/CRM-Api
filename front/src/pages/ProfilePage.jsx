import { useState, useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { apiClient } from "../api/apiClient";
import { logout } from "../store/authSlice";
import profileBig from "../assets/profiles/profileBig.png";

export default function ProfilePage() {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const user = useSelector((state) => state.auth.user);

  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    phone: "",
    role: "",
    password: "",
    confirmPassword: "",
  });

  useEffect(() => {
    if (user) {
      const displayRole = user.role?.name
        ? user.role.name
        : user.isSystemAdmin
          ? "Системний Адміністратор"
          : "Співробітник";

      setFormData((prev) => ({
        ...prev,
        fullName: user.fullName || "",
        email: user.email || "",
        phone: user.phoneNumber || "",
        role: displayRole,
      }));
    }
  }, [user]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (formData.password && formData.password !== formData.confirmPassword) {
      alert("Паролі не співпадають!");
      return;
    }

    try {
      console.log("Дані для відправки на сервер:", formData);
      alert(
        "Дані успішно збережено! (Очікуємо ендпоінт від бекенда для оновлення)",
      );

      setFormData((prev) => ({ ...prev, password: "", confirmPassword: "" }));
    } catch (error) {
      console.error("Помилка збереження:", error);
      alert("Не вдалося зберегти дані.");
    }
  };

  const handleDeleteAccount = async () => {
    if (
      window.confirm(
        "Ви впевнені, що хочете видалити свій акаунт? Цю дію неможливо скасувати.",
      )
    ) {
      try {
        if (user?.id) {
          await apiClient.delete(`/api/user/${user.id}`);
        }
        dispatch(logout());
        navigate("/login");
      } catch (error) {
        console.error("Помилка видалення акаунта:", error);
        alert("Не вдалося видалити акаунт.");
      }
    }
  };

  return (
    <div className="w-full">
      <div className="bg-falcon-dark rounded-xl max-w-4xl mx-auto p-5 flex flex-col items-center shadow-lg">
        <div className="relative w-48 h-48 mb-5">
          <img
            src={profileBig}
            alt="Profile"
            className="w-full h-full object-cover rounded-3xl"
          />
          <button className="absolute -bottom-2 -right-2 w-10 h-10 bg-white rounded-full flex items-center justify-center text-gray-500 hover:text-falcon-dark shadow-md transition-transform hover:scale-110">
            <span className="text-2xl font-light leading-none">+</span>
          </button>
        </div>

        <form
          onSubmit={handleSubmit}
          className="w-full max-w-2xl flex flex-col items-center"
        >
          <div className="grid grid-cols-2 gap-x-6 gap-y-4 w-full mb-5">
            {/* Лейбл 1: ПІБ */}
            <label className="flex flex-col w-full">
              <span className="text-falcon-light/80 text-sm mb-1.5 ml-1 font-medium tracking-wide">
                Прізвище та ім'я
              </span>
              <input
                type="text"
                name="fullName"
                placeholder="Введіть ваше ім'я"
                value={formData.fullName}
                onChange={handleChange}
                className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-5 py-3 rounded-md w-full focus:outline-none focus:ring-1 focus:ring-white transition-shadow"
              />
            </label>

            {/* Лейбл 2: Email */}
            <label className="flex flex-col w-full">
              <span className="text-falcon-light/80 text-sm mb-1.5 ml-1 font-medium tracking-wide">
                Електронна пошта
              </span>
              <input
                type="email"
                name="email"
                placeholder="email@example.com"
                value={formData.email}
                onChange={handleChange}
                className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-5 py-3 rounded-md w-full focus:outline-none focus:ring-1 focus:ring-white transition-shadow"
              />
            </label>

            {/* Лейбл 3: Телефон */}
            <label className="flex flex-col w-full">
              <span className="text-falcon-light/80 text-sm mb-1.5 ml-1 font-medium tracking-wide">
                Мобільний телефон
              </span>
              <input
                type="tel"
                name="phone"
                placeholder="+380 00 000 00 00"
                value={formData.phone}
                onChange={handleChange}
                className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-5 py-3 rounded-md w-full focus:outline-none focus:ring-1 focus:ring-white transition-shadow"
              />
            </label>

            {/* Лейбл 4: Роль */}
            <label className="flex flex-col w-full">
              <span className="text-falcon-light/80 text-sm mb-1.5 ml-1 font-medium tracking-wide">
                Посада (Роль)
              </span>
              <input
                type="text"
                name="role"
                value={formData.role}
                disabled
                className="bg-falcon-light/70 text-falcon-dark placeholder-falcon-dark/50 px-5 py-3 rounded-md w-full focus:outline-none cursor-not-allowed"
              />
            </label>

            {/* Лейбл 5: Пароль */}
            <label className="flex flex-col w-full mt-4">
              <span className="text-falcon-light/80 text-sm mb-1.5 ml-1 font-medium tracking-wide">
                Новий пароль
              </span>
              <input
                type="password"
                name="password"
                placeholder="Залиште пустим, якщо не міняєте"
                value={formData.password}
                onChange={handleChange}
                className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-5 py-3 rounded-md w-full focus:outline-none focus:ring-1 focus:ring-white transition-shadow"
              />
            </label>

            {/* Лейбл 6: Підтвердження паролю */}
            <label className="flex flex-col w-full mt-4">
              <span className="text-falcon-light/80 text-sm mb-1.5 ml-1 font-medium tracking-wide">
                Підтвердження паролю
              </span>
              <input
                type="password"
                name="confirmPassword"
                placeholder="Повторіть новий пароль"
                value={formData.confirmPassword}
                onChange={handleChange}
                className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-5 py-3 rounded-md w-full focus:outline-none focus:ring-1 focus:ring-white transition-shadow"
              />
            </label>
          </div>

          <button
            type="submit"
            className="bg-falcon-light text-falcon-dark text-xl font-medium px-16 py-3 rounded hover:brightness-105 transition-all shadow-[0_4px_0_0_rgba(0,0,0,0.1)] active:shadow-none active:translate-y-[1px]"
          >
            Зберегти
          </button>

          <button
            type="button"
            onClick={handleDeleteAccount}
            className="mt-6 text-falcon-light/60 hover:text-red-300 text-sm transition-colors border-b border-falcon-light/20 hover:border-red-300 pb-0.5"
          >
            Видалити акаунт
          </button>
        </form>
      </div>
    </div>
  );
}
