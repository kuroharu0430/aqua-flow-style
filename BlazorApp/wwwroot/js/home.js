if (window.location.pathname === "/") {

    document.addEventListener('DOMContentLoaded', () => {
        // 中断しないとバグるので注意
        return

        setTimeout(() => {
            const ripple1 = document.querySelector('.ripple1');
            ripple1.style.width = '600px';
            ripple1.style.height = '600px';
            /*        ripple1.style.backgroundColor = '#f0f8ff';*/
            ripple1.style.transition = 'width 2.5s ease-out, height 2.5s ease-out, opacity 2.5s ease-out';
            ripple1.style.opacity = '0'; // フェードアウト
        }, 1200);

        setTimeout(() => {
            const ripple2 = document.querySelector('.ripple2');
            ripple2.style.width = '1200px';
            ripple2.style.height = '1200px';
            /*        ripple2.style.backgroundColor = '#f0f8ff';*/
            ripple2.style.transition = 'width 2.5s ease-out, height 2.5s ease-out, opacity 2.5s ease-out';
            ripple2.style.opacity = '0'; // フェードアウト
        }, 2200);

        // コンテンツの表示（clip-path拡張）
        setTimeout(() => {
            const content = document.querySelector('.masked-content');
            content.classList.add('reveal');
            content.style.clipPath = 'circle(150% at center)';
        }, 2500);

        // グレースケール解除（少し遅らせる）
        setTimeout(() => {
            const content = document.querySelector('.masked-content');
            content.style.filter = 'grayscale(0%)';
            content.style.opacity = '1';
        }, 4000); // clip-path完了後に色が戻るように

        setTimeout(() => {
            const text = 'Aqua Flow Style';
            const typed = document.querySelector('.typed');
            const cursor = document.querySelector('.cursor');

            let index = 0;

            const typeInterval = setInterval(() => {
                if (index < text.length) {
                    typed.textContent += text.charAt(index);
                    index++;
                } else {
                    clearInterval(typeInterval);

                    // カーソルをフェードアウト
                    setTimeout(() => {
                        cursor.classList.add('fade-out');
                    }, 500); // 少し余韻を残してから消える
                }
            }, 150);
        }, 4500);
    });
}
