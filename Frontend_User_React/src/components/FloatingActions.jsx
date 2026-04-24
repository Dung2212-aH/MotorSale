function FloatingActions() {
  return (
    <div className="fixed right-4 bottom-4 z-20 grid gap-3" aria-label="Liên hệ nhanh">
      <a
        className="grid h-12 w-12 place-items-center rounded-full bg-[#d71920] text-xs font-black uppercase tracking-[0.08em] text-white shadow-[0_14px_30px_rgba(15,23,42,0.22)] transition hover:-translate-y-1"
        href="tel:19006750"
        aria-label="Gọi hotline"
      >
        Tel
      </a>
      <a
        className="grid h-12 w-12 place-items-center rounded-full bg-zinc-950 text-xs font-black uppercase tracking-[0.08em] text-white shadow-[0_14px_30px_rgba(15,23,42,0.22)] transition hover:-translate-y-1"
        href="https://m.me/"
        target="_blank"
        rel="noreferrer"
        aria-label="Messenger"
      >
        Msg
      </a>
    </div>
  );
}

export default FloatingActions;
