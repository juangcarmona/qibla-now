// Markdown content modules — lazily loaded (project-root-absolute paths)
const allMd = import.meta.glob('/src/content/**/*.md');

// UI JSON modules — eagerly loaded
const allUI = import.meta.glob('/src/content/**/ui.json', { eager: true });

export async function getPageContent(lang: string, page: string) {
  const mdKey = `/src/content/${lang}/${page}.md`;
  const enKey = `/src/content/en/${page}.md`;
  const hasLang = mdKey in allMd;
  const loader = allMd[hasLang ? mdKey : enKey];

  if (!loader) {
    return { frontmatter: {} as Record<string, string>, Content: null, isFallback: false };
  }

  const mod = await loader() as any;
  return {
    frontmatter: (mod.frontmatter ?? {}) as Record<string, string>,
    Content: mod.Content as any,
    isFallback: !hasLang && lang !== 'en',
  };
}

export function getUI(lang: string): Record<string, any> {
  const key = `/src/content/${lang}/ui.json`;
  const fallback = '/src/content/en/ui.json';
  const raw = (allUI[key] ?? allUI[fallback]) as any;
  return raw?.default ?? raw ?? {};
}
