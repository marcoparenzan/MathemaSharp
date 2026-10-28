class DocumentationBrowser {
    enhance() {
        const article = document.querySelector('.markdown');
        const headings = article ? [...article.querySelectorAll('h2, h3')] : [];
        const signature = headings.map(h => h.textContent).join('|');
        const existing = document.querySelector('.document-toc');
        if (!article) existing?.remove();
        else if (!existing || existing.dataset.signature !== signature || headings.some(h => !h.id)) {
            existing?.remove();
            if (headings.length > 1) {
                const toc = document.createElement('details');
                toc.className = 'document-toc';
                toc.dataset.signature = signature;
                const summary = document.createElement('summary');
                summary.textContent = 'In questa pagina';
                const list = document.createElement('ol');
                const used = new Set([...document.querySelectorAll('[id]')].map(node => node.id));
                headings.forEach((heading, index) => {
                    if (!heading.id) {
                        let id = `section-${index + 1}`;
                        while (used.has(id)) id += '-';
                        heading.id = id;
                        used.add(id);
                    }
                    const item = document.createElement('li');
                    const link = document.createElement('a');
                    link.href = '#' + encodeURIComponent(heading.id);
                    link.textContent = heading.textContent;
                    if (heading.tagName === 'H3') item.className = 'toc-subheading';
                    item.append(link);
                    list.append(item);
                });
                toc.append(summary, list);
                article.before(toc);
            }
        }
        document.querySelectorAll('.markdown pre').forEach(pre => {
            if (pre.querySelector('button')) return;
            const button = document.createElement('button');
            button.className = 'copy-code';
            button.textContent = 'Copia';
            button.type = 'button';
            button.addEventListener('click', async () => {
                try {
                    await navigator.clipboard.writeText(pre.querySelector('code')?.textContent ?? '');
                    button.textContent = 'Copiato';
                } catch { button.textContent = 'Copia non disponibile'; }
                setTimeout(() => button.textContent = 'Copia', 2000);
            });
            pre.append(button);
        });
        document.querySelectorAll('.markdown a').forEach(link => {
            if (link.origin !== location.origin) { link.target = '_blank'; link.rel = 'noopener noreferrer'; }
        });
    }
    print() { window.print(); }
}
window.numbersDocs = new DocumentationBrowser();
