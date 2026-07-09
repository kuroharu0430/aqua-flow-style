if (window.location.pathname === "/") {

    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(() => {
            const ripple1 = document.querySelector('.ripple1');
            if (!ripple1) return;
            ripple1.style.width = '600px';
            ripple1.style.height = '600px';
            ripple1.style.transition = 'width 2.5s ease-out, height 2.5s ease-out, opacity 2.5s ease-out';
            ripple1.style.opacity = '0';
        }, 1200);

        setTimeout(() => {
            const ripple2 = document.querySelector('.ripple2');
            if (!ripple2) return;
            ripple2.style.width = '1200px';
            ripple2.style.height = '1200px';
            ripple2.style.transition = 'width 2.5s ease-out, height 2.5s ease-out, opacity 2.5s ease-out';
            ripple2.style.opacity = '0';
        }, 2200);

        setTimeout(() => {
            const content = document.querySelector('.masked-content');
            if (!content) return;
            content.classList.add('reveal');
            content.style.clipPath = 'circle(150% at center)';
        }, 2500);

        setTimeout(() => {
            const content = document.querySelector('.masked-content');
            if (!content) return;
            content.style.filter = 'grayscale(0%)';
            content.style.opacity = '1';
        }, 4000);

        setTimeout(() => {
            const text = 'Aqua Flow Style';
            const typed = document.querySelector('.typed');
            const cursor = document.querySelector('.cursor');

            if (!typed || !cursor) return;

            let index = 0;

            const typeInterval = setInterval(() => {
                if (!typed || !cursor) {
                    clearInterval(typeInterval);
                    return;
                }

                if (index < text.length) {
                    typed.textContent += text.charAt(index);
                    index++;
                } else {
                    clearInterval(typeInterval);
                    setTimeout(() => cursor.classList.add('fade-out'), 500);
                }
            }, 150);
        }, 4500);
    });
}
