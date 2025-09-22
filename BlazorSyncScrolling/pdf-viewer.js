async function loadPdf(url, containerId, initialScale) {
    const pdfjsLib = window['pdfjsLib'];

    if (!pdfjsLib) {
        console.error("PDF.js is not loaded. Make sure to include the PDF.js script.");
        return;
    }
    pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.0.375/pdf.worker.min.mjs';

    const container = document.getElementById(containerId);
    container.innerHTML = ''; 

    if (url.startsWith('data:application/pdf;base64,')) {
        url = base64ToUint8Array(url.replace('data:application/pdf;base64,', ''));
    }
    const loadingTask = pdfjsLib.getDocument(url);
    const pdf = await loadingTask.promise;

    const canvases = [];
    let isRendering = false;

    const userScale = initialScale ? true : false;
    const standardPdfWidth = 800;
    let scale = initialScale || (container.clientWidth / standardPdfWidth);

    function base64ToUint8Array(base64) {
        const raw = atob(base64);
        const uint8Array = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; i++) {
            uint8Array[i] = raw.charCodeAt(i);
        }
        return uint8Array;
    }

    async function renderPage(pageNum) {
        if (isRendering) return;
        isRendering = true;

        const page = await pdf.getPage(pageNum);
        const viewport = page.getViewport({ scale });

        let canvas = canvases[pageNum - 1];
        if (!canvas) {
            canvas = document.createElement('canvas');
            container.appendChild(canvas);
            canvases[pageNum - 1] = canvas;

            canvas.style.marginBottom = '10px';
            canvas.style.borderBottom = '2px solid #ccc';
        }

        canvas.height = viewport.height;
        canvas.width = viewport.width;

        const context = canvas.getContext('2d');
        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };

        await page.render(renderContext).promise;
        isRendering = false;
    }

    for (let i = 1; i <= pdf.numPages; i++) {
        await renderPage(i);
    }

    function getCurrentPage() {
        const visibleHeight = container.clientHeight;
        let closestPage = null;
        let closestDistance = Infinity;

        for (let i = 1; i <= pdf.numPages; i++) {
            const canvas = canvases[i - 1];
            if (canvas) {
                const rect = canvas.getBoundingClientRect();
                const distanceFromCenter = Math.abs(rect.top + rect.height / 2 - visibleHeight / 2);

                // If the page is visible, get the closest page
                if (rect.top >= -visibleHeight && rect.top <= visibleHeight) {
                    if (distanceFromCenter < closestDistance) {
                        closestDistance = distanceFromCenter;
                        closestPage = i;
                    }
                }
            }
        }

        // If no page is visible, get the page at the top or bottom
        if (closestPage === null) {
            for (let i = 1; i <= pdf.numPages; i++) {
                const canvas = canvases[i - 1];
                if (canvas) {
                    const rect = canvas.getBoundingClientRect();

                    // Get the page at the top
                    if (rect.top < 0) {
                        closestPage = i;
                    }

                    // Get the page at the bottom
                    if (rect.top > 0) {
                        closestPage = i;
                        break;
                    }
                }
            }
        }

        return closestPage || 1;
    }

    async function updateScale(newScale) {
        scale = newScale;
        for (let i = 1; i <= pdf.numPages; i++) {
            await renderPage(i);
        }
    }

    container.addEventListener('scroll', async () => {
        const visibleHeight = container.clientHeight;
        for (let i = 1; i <= pdf.numPages; i++) {
            const canvas = canvases[i - 1];
            if (canvas) {
                const rect = canvas.getBoundingClientRect();
                if (rect.top >= -visibleHeight && rect.top <= visibleHeight) {
                    if (!canvas.classList.contains('rendered')) {
                        await renderPage(i);
                        canvas.classList.add('rendered');
                    }
                }
            }
        }
    });

    window.addEventListener('resize', async () => {
        scale = userScale ? scale : container.clientWidth / standardPdfWidth; 
        for (let i = 1; i <= pdf.numPages; i++) {
            await renderPage(i);
        }
    });

    return {
        getNumPages: () => pdf.numPages,
        getCurrentPage,
        updateScale
    };
}

/**
 * 
 * @param {[]} ids element ids that can be related to same sync scroll
 * @returns
 */
function scrollSynchronizer(ids) {
    const scrollHandlers = {};
    const programmaticScroll = {};

    ids.forEach(id => {
        scrollHandlers[id] = {};
        programmaticScroll[id] = false;
    });

    function scrollElem(fromId, toId) {
        const fromEl = document.getElementById(fromId);
        const toEl = document.getElementById(toId);

        if (!fromEl || !toEl) return;

        programmaticScroll[toId] = true;
        toEl.scrollTop = fromEl.scrollTop;
        toEl.scrollLeft = fromEl.scrollLeft;
    }

    function enableSyncScroll(activeIds) {
        ids.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;

            Object.keys(scrollHandlers[id]).forEach(otherId => {
                el.removeEventListener('scroll', scrollHandlers[id][otherId]);
            });
            scrollHandlers[id] = {};
        });

        activeIds.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;

            activeIds.forEach(otherId => {
                if (id === otherId) return;

                const handler = () => {
                    if (programmaticScroll[id]) {
                        programmaticScroll[id] = false;
                        return;
                    }

                    scrollElem(id, otherId);
                };

                el.addEventListener('scroll', handler);
                scrollHandlers[id][otherId] = handler;
            });
        });
    }

    return {
        enableSyncScroll
    };
}

function registerScrollHandler(containerId, dotNetHelper) {
    const container = document.getElementById(containerId);
    if (!container)
        return;
    container.addEventListener('scroll', async () => {
        const currentPage = dotNetHelper.invokeMethodAsync('OnScroll');
    });
}