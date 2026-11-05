export const ui = {
    showToast(message, type = 'success') {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        container.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 3500);
    },

    showLoading(containerElement, message = 'Loading...') {
        containerElement.innerHTML = `
            <div class="loading-state">
                <div class="spinner"></div>
                <p>${message}</p>
            </div>
        `;
    },

    showEmptyState(containerElement, title, description) {
        containerElement.innerHTML = `
            <div class="empty-state">
                <h3>${title}</h3>
                <p>${description}</p>
            </div>
        `;
    }
};
