// Открытие модального окна
document.querySelectorAll('.open-modal').forEach(button => {
    button.addEventListener('click', (event) => {
        event.preventDefault(); // Чтобы ссылка не перезагружала страницу
        document.getElementById('modal-overlay').style.display = 'block';
    });
});

// Закрытие модального окна
document.getElementById('close-modal').addEventListener('click', () => {
    document.getElementById('modal-overlay').style.display = 'none';
});

// Закрытие при клике вне модального окна
document.getElementById('modal-overlay').addEventListener('click', (event) => {
    if (event.target === document.getElementById('modal-overlay')) {
        document.getElementById('modal-overlay').style.display = 'none';
    }
});
