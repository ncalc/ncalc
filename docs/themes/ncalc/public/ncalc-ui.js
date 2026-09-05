const searchDialog = document.getElementById("search-dialog")
const searchTrigger = document.getElementById("search-trigger")
const searchClose = document.getElementById("search-dialog-close")
const searchInput = document.getElementById("search-query")
const searchResults = document.getElementById("search-results")
const platformKey = document.querySelector(".platform-key")

if (platformKey) {
  platformKey.textContent = /Mac|iPhone|iPad/.test(navigator.platform) ? "⌘" : "Ctrl"
}

if (searchDialog && searchTrigger && searchInput && searchResults) {
  let activeIndex = -1
  let focusTimer

  const getResultLinks = () => Array.from(searchResults.querySelectorAll("a[href]"))

  const setActiveResult = index => {
    const links = getResultLinks()
    for (const link of links) {
      link.classList.remove("is-active")
    }

    if (links.length === 0) {
      activeIndex = -1
      return
    }

    activeIndex = (index + links.length) % links.length
    const activeLink = links[activeIndex]
    activeLink.classList.add("is-active")
    activeLink.scrollIntoView({ block: "nearest" })
  }

  const focusSearchWhenReady = () => {
    window.clearTimeout(focusTimer)
    if (!searchInput.disabled) {
      searchInput.focus()
      return
    }

    focusTimer = window.setTimeout(focusSearchWhenReady, 50)
  }

  const openSearch = () => {
    if (!searchDialog.open) {
      searchDialog.showModal()
    }
    focusSearchWhenReady()
  }

  const closeSearch = () => {
    if (searchDialog.open) {
      searchDialog.close()
    }
  }

  searchTrigger.addEventListener("click", openSearch)
  searchClose?.addEventListener("click", closeSearch)

  searchDialog.addEventListener("click", event => {
    if (event.target === searchDialog) {
      closeSearch()
    }
  })

  searchDialog.addEventListener("close", () => {
    window.clearTimeout(focusTimer)
    activeIndex = -1
    searchInput.value = ""
    searchInput.dispatchEvent(new Event("input", { bubbles: true }))
    searchTrigger.focus()
  })

  searchDialog.addEventListener("keydown", event => {
    if (event.key === "ArrowDown") {
      event.preventDefault()
      setActiveResult(activeIndex + 1)
    } else if (event.key === "ArrowUp") {
      event.preventDefault()
      setActiveResult(activeIndex - 1)
    } else if (event.key === "Enter" && activeIndex >= 0) {
      event.preventDefault()
      getResultLinks()[activeIndex]?.click()
    }
  })

  document.addEventListener("keydown", event => {
    if (event.key.toLowerCase() === "k" && (event.metaKey || event.ctrlKey)) {
      event.preventDefault()
      openSearch()
    }
  })

  new MutationObserver(() => {
    activeIndex = -1
  }).observe(searchResults, { childList: true, subtree: true })
}
