import { createRouter, createWebHistory } from "vue-router"

/* Layouts */
import AuthLayout from "@/layouts/AuthLayout.vue"
import DashboardLayout from "@/layouts/DashboardLayout.vue"
import CentralLayout from "@/layouts/CentralLayout.vue"
import CompanyLayout from "@/layouts/CompanyLayout.vue"

/* Auth Pages */
import Login from "@/views/Login.vue"
import Signup from "@/views/Signup.vue"

/* App Pages (Normal User) */
import Dashboard from "@/views/Dashboard.vue"
import Profile from "@/views/Profile.vue"
import Jobs from "@/views/Jobs.vue"
import Applications from "@/views/Applications.vue"
import AiCareer from "@/views/AiCareer.vue"
import Chat from "@/views/Chat.vue"

/* Central Recruiter Pages */
import CentralDashboard from "@/views/central/Dashboard.vue"
import CentralJobs from "@/views/central/Jobs.vue"
import CentralApplications from "@/views/central/Applications.vue"
import CentralUsers from "@/views/central/Users.vue"
import CentralReports from "@/views/central/Reports.vue"

/* Company Recruiter Pages */
import CompanyDashboard from "@/views/company/Dashboard.vue"
import SetupCompany from "@/views/company/SetupCompany.vue"

/* 🔐 API base */
const BASE = "https://localhost:7269/api"

const routes = [
  {
    path: "/",
    redirect: "/login"
  },

  /* AUTH ROUTES */
  {
    path: "/login",
    component: AuthLayout,
    children: [{ path: "", component: Login }]
  },
  {
    path: "/signup",
    component: AuthLayout,
    children: [{ path: "", component: Signup }]
  },

  /* NORMAL USER ROUTES */
  {
    path: "/app",
    component: DashboardLayout,
    children: [
      { path: "", component: Dashboard },
      { path: "profile", component: Profile },
      { path: "jobs", component: Jobs },
      { path: "applications", component: Applications },
      { path: "ai", component: AiCareer },
      { path: "chat", component: Chat }
    ]
  },

  /* CENTRAL RECRUITER ROUTES */
  {
    path: "/central",
    component: CentralLayout,
    children: [
      { path: "dashboard", component: CentralDashboard },
      { path: "jobs", component: CentralJobs },
      { path: "applications", component: CentralApplications },
      { path: "users", component: CentralUsers },
      { path: "reports", component: CentralReports }
    ]
  },

  /* COMPANY RECRUITER ROUTES */
  {
    path: "/company",
    component: CompanyLayout,
    children: [
      { path: "dashboard", component: CompanyDashboard },
      { path: "setup", component: SetupCompany }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

/* 🔐 SESSION + ROLE-AWARE ROUTE GUARD */
router.beforeEach(async (to, from, next) => {
  const isLoggedIn = localStorage.getItem("isLoggedIn")
  const role = localStorage.getItem("userRole")

  const isAuthPage = to.path === "/login" || to.path === "/signup"
  const isUserPage = to.path.startsWith("/app")
  const isCentralPage = to.path.startsWith("/central")
  const isCompanyPage = to.path.startsWith("/company")

  // 🚫 Not logged in → block protected routes
  if ((isUserPage || isCentralPage || isCompanyPage) && !isLoggedIn) {
    return next("/login")
  }

  // 🔁 Already logged in → block login/signup
  if (isAuthPage && isLoggedIn) {
    if (role === "Central")
      return next("/central/dashboard")
    else if (role === "Company")
      return next("/company/dashboard")
    else
      return next("/app")
  }

  // 🔐 Role Protection
  if (isUserPage && role === "Central") {
    return next("/central/dashboard")
  }

  if (isCentralPage && role !== "Central") {
    return next("/app")
  }

  if (isCompanyPage && role !== "Company") {
    return next("/app")
  }

  // 🔐 Backend session verification
  if (isUserPage || isCentralPage || isCompanyPage) {
    try {
      const res = await fetch(`${BASE}/auth/check-session`, {
        credentials: "include"
      })

      if (!res.ok) {
        localStorage.clear()
        return next("/login")
      }

      const userData = await res.json()

      // 🚩 Company Onboarding Guard
      if (role === "Company" && !userData.companyId && to.path !== "/company/setup") {
        return next("/company/setup")
      }

      // Prevent setup page if already joined
      if (role === "Company" && userData.companyId && to.path === "/company/setup") {
        return next("/company/dashboard")
      }

    } catch (error) {
      localStorage.clear()
      return next("/login")
    }
  }

  next()
})

export default router