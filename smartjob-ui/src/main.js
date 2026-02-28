import { createApp } from "vue"
import App from "./App.vue"
import router from "./router"

// Bootstrap
import "bootstrap/dist/css/bootstrap.min.css"
import "bootstrap"

// Global CSS
import "@/assets/main.css"

/* ===============================
   🔥 THEME INITIALIZATION (FIX)
   =============================== */

// Get saved theme or default to dark
const savedTheme = localStorage.getItem("theme") || "theme-dark"

// ⚠️ APPLY THEME TO <BODY> — NOT #app
document.body.classList.remove("theme-light", "theme-dark")
document.body.classList.add(savedTheme)

/* ===============================
   🚀 CREATE APP
   =============================== */

createApp(App)
  .use(router)
  .mount("#app")
