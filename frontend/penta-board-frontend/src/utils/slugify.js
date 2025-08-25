export default function slugify(text) {
  return (text || '')
    .toString()
    .normalize('NFKD')
    .replace(/[\u0300-\u036f]/g, '') // accent'leri at
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')     // boşluk/özel -> -
    .replace(/^-+|-+$/g, '')         // baş/son -
    .slice(0, 50);
}
