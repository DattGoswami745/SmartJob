import { createApp } from "vue"
import App from "./App.vue"
import router from "./router"

// Bootstrap
import "bootstrap/dist/css/bootstrap.min.css"
import "bootstrap-icons/font/bootstrap-icons.css"
import "bootstrap"

// Global CSS
import "@/assets/main.css"

/* ===============================
   🔥 THEME INITIALIZATION (FIXED: ALWAYS WHITE)
   =============================== */

// Force white theme for all users
document.body.classList.remove("theme-dark")
document.body.classList.add("theme-light")

/* ===============================
   🚀 CREATE APP
   =============================== */

createApp(App)
   .use(router)
   .mount("#app")
