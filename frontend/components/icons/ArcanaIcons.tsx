import type { SVGProps } from "react";

type IconProps = SVGProps<SVGSVGElement>;

export function BrandMarkIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 40 40" fill="none" aria-hidden="true" {...props}>
      <path d="M20 2v36M2 20h36M7.3 7.3l25.4 25.4M32.7 7.3 7.3 32.7" stroke="currentColor" strokeWidth="1.1" strokeLinecap="round" />
      <path d="M20 8.5 23 17l8.5 3-8.5 3-3 8.5-3-8.5-8.5-3 8.5-3 3-8.5Z" fill="currentColor" fillOpacity=".15" stroke="currentColor" strokeWidth="1.35" />
      <path d="m20 13 1.7 5.3L27 20l-5.3 1.7L20 27l-1.7-5.3L13 20l5.3-1.7L20 13Z" fill="currentColor" />
      <circle cx="20" cy="2.8" r="1.25" fill="currentColor" />
      <circle cx="37.2" cy="20" r="1.25" fill="currentColor" />
      <circle cx="20" cy="37.2" r="1.25" fill="currentColor" />
      <circle cx="2.8" cy="20" r="1.25" fill="currentColor" />
    </svg>
  );
}

export function SparkleIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="M12 2.7c.8 4.7 2.7 6.7 7.3 7.5-4.6.8-6.5 2.8-7.3 7.5-.8-4.7-2.7-6.7-7.3-7.5 4.6-.8 6.5-2.8 7.3-7.5Z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M19.4 3.8c.2 1.4.8 2 2.2 2.3-1.4.3-2 1-2.2 2.3-.2-1.4-.8-2-2.2-2.3 1.4-.3 2-1 2.2-2.3Z" fill="currentColor" />
    </svg>
  );
}

export function NewReadingIcon(props: IconProps) {
  return <SparkleIcon {...props} />;
}

export function ReadingsIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="M4 5.3c2.8-.9 5.4-.5 8 1.1v12.1c-2.6-1.6-5.2-2-8-1.1V5.3Z" stroke="currentColor" strokeWidth="1.65" strokeLinejoin="round" />
      <path d="M20 5.3c-2.8-.9-5.4-.5-8 1.1v12.1c2.6-1.6 5.2-2 8-1.1V5.3ZM7 9h2.3M7 12h2.3M14.7 9H17M14.7 12H17" stroke="currentColor" strokeWidth="1.65" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}


export function ChatIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="M5.3 5.2h13.4A2.3 2.3 0 0 1 21 7.5v7.2a2.3 2.3 0 0 1-2.3 2.3H11l-4.7 3.2.9-3.2H5.3A2.3 2.3 0 0 1 3 14.7V7.5a2.3 2.3 0 0 1 2.3-2.3Z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M8 11h.01M12 11h.01M16 11h.01" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" />
    </svg>
  );
}

export function AboutIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <circle cx="12" cy="12" r="8.3" stroke="currentColor" strokeWidth="1.7" />
      <path d="M12 10.6v5.2M12 7.7h.01" stroke="currentColor" strokeWidth="1.9" strokeLinecap="round" />
    </svg>
  );
}

export function SearchIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <circle cx="10.7" cy="10.7" r="6.2" stroke="currentColor" strokeWidth="1.8" />
      <path d="m15.4 15.4 4.1 4.1" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

export function ChevronDownIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="m7 9.5 5 5 5-5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export function ChevronRightIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="m9.5 6 6 6-6 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export function ArrowRightIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="M5 12h14M14 7l5 5-5 5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export function ArrowLeftIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="M19 12H5M10 7l-5 5 5 5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export function MenuIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="M4 7h16M4 12h16M4 17h16" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

export function CloseIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="m6 6 12 12M18 6 6 18" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

export function EditIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path d="m5 16.8-.7 3 3-.7L18.2 8.2 15.8 5.8 5 16.8Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
      <path d="m14.6 7 2.4 2.4" stroke="currentColor" strokeWidth="1.7" />
    </svg>
  );
}

export function QuestionIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <circle cx="16" cy="16" r="11.5" stroke="currentColor" strokeWidth="1.5" />
      <path d="M12.7 12.3a3.7 3.7 0 0 1 7.2 1.2c0 2.6-3.9 3.1-3.9 5.5M16 23.3h.01" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

export function DrawCardsIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <rect x="7" y="6" width="15" height="20" rx="2" stroke="currentColor" strokeWidth="1.5" />
      <path d="m12 6 11.5-2a2 2 0 0 1 2.3 1.6l2.3 13.2a2 2 0 0 1-1.6 2.3L22 22" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
      <path d="M14.5 11.5c.4 2.5 1.4 3.5 3.8 4-2.4.4-3.4 1.5-3.8 4-.4-2.5-1.4-3.6-3.8-4 2.4-.5 3.4-1.5 3.8-4Z" stroke="currentColor" strokeWidth="1.2" />
    </svg>
  );
}

export function ViewReadingIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <path d="M6.5 6.5c3.2-1 6.4-.5 9.5 1.5v18c-3.1-2-6.3-2.5-9.5-1.5v-18Z" stroke="currentColor" strokeWidth="1.5" />
      <path d="M25.5 6.5c-3.2-1-6.4-.5-9.5 1.5v18c3.1-2 6.3-2.5 9.5-1.5v-18Z" stroke="currentColor" strokeWidth="1.5" />
      <path d="m16 12 1.2 3.2 3.3 1.2-3.3 1.2L16 21l-1.2-3.4-3.3-1.2 3.3-1.2L16 12Z" stroke="currentColor" strokeWidth="1.2" />
    </svg>
  );
}

export function ChooseSpreadIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <rect x="7" y="5" width="18" height="22" rx="2" stroke="currentColor" strokeWidth="1.5" />
      <path d="M11 9h10v14H11z" stroke="currentColor" strokeWidth="1.1" />
      <path d="m16 11 1.4 3.6 3.6 1.4-3.6 1.4L16 21l-1.4-3.6L11 16l3.6-1.4L16 11Z" stroke="currentColor" strokeWidth="1.1" />
    </svg>
  );
}

export function MissionIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <path d="M10 7.5h12v18H10z" stroke="currentColor" strokeWidth="1.5" />
      <path d="M13 7.5V5h6v2.5M16 11v10M12.5 16H19.5M16 11l2.2 3.2L16 16l-2.2-1.8L16 11Z" stroke="currentColor" strokeWidth="1.35" strokeLinejoin="round" />
    </svg>
  );
}

export function CardsIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <rect x="7.5" y="6" width="16" height="21" rx="2" stroke="currentColor" strokeWidth="1.5" />
      <path d="m16 10 1.7 4.3L22 16l-4.3 1.7L16 22l-1.7-4.3L10 16l4.3-1.7L16 10Z" stroke="currentColor" strokeWidth="1.2" />
      <path d="m11 6 12-2" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" />
    </svg>
  );
}

export function JourneyIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <path d="M10 26c5-2 3.2-6.2 7-8 3.8-1.8 3.2-6 5-9" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
      <circle cx="22" cy="7" r="3" stroke="currentColor" strokeWidth="1.5" />
      <path d="m8 25 4 1-2.5 3.5L8 25Z" stroke="currentColor" strokeWidth="1.3" strokeLinejoin="round" />
      <path d="M14 13c.3 2 1.1 2.8 3 3.2-1.9.4-2.7 1.2-3 3.2-.3-2-1.1-2.8-3-3.2 1.9-.4 2.7-1.2 3-3.2Z" stroke="currentColor" strokeWidth="1.15" />
    </svg>
  );
}


export function CalendarIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <rect x="6" y="8" width="20" height="18" rx="2.5" stroke="currentColor" strokeWidth="1.6" />
      <path d="M10 5v6M22 5v6M6 13h20" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
      <path d="M11 17h3M18 17h3M11 21h3M18 21h3" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
    </svg>
  );
}

export function SpreadIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <path d="m16 5 7 11-7 11L9 16 16 5Z" stroke="currentColor" strokeWidth="1.5" />
      <circle cx="16" cy="16" r="2.5" fill="currentColor" />
    </svg>
  );
}

export function InterpretationSpaceIllustration(props: IconProps) {
  return (
    <svg viewBox="0 0 280 190" fill="none" aria-hidden="true" {...props}>
      <g stroke="currentColor" opacity=".7">
        <path d="M140 34v54M113 61h54M121 42l38 38M159 42l-38 38" strokeWidth="1.2" />
        <path d="m140 47 5 14 14 5-14 5-5 14-5-14-14-5 14-5 5-14Z" fill="currentColor" fillOpacity=".13" strokeWidth="1.2" />
        <path d="M53 158c25-15 52-17 82-4V99c-29-13-57-11-82 4v55ZM227 158c-25-15-52-17-82-4V99c29-13 57-11 82 4v55Z" strokeWidth="1.6" />
        <path d="M53 158c31-4 61 0 87 16M227 158c-31-4-61 0-87 16" strokeWidth="1.2" />
        <path d="M72 113c18-5 34-5 49 1M72 126c18-5 34-5 49 1M208 113c-18-5-34-5-49 1M208 126c-18-5-34-5-49 1" strokeWidth="1" opacity=".65" />
      </g>
    </svg>
  );
}

export function TarotCardBack(props: IconProps) {
  return (
    <svg viewBox="0 0 62 92" fill="none" aria-hidden="true" {...props}>
      <rect x="1" y="1" width="60" height="90" rx="6" fill="#0b0c1c" stroke="#9b7548" strokeWidth="1.6" />
      <rect x="5.5" y="5.5" width="51" height="81" rx="3.5" stroke="#5c452f" strokeWidth="1" />
      <rect x="9" y="9" width="44" height="74" rx="2.5" stroke="#997246" strokeWidth=".7" opacity=".7" />
      <path d="M31 14v64M14 46h34M19 25l24 42M43 25 19 67" stroke="#866440" strokeWidth=".8" strokeLinecap="round" opacity=".55" />
      <circle cx="31" cy="46" r="13" stroke="#9c774a" strokeWidth=".7" opacity=".65" />
      <path d="m31 30.5 3.7 11.8L46 46l-11.3 3.7L31 61.5l-3.7-11.8L16 46l11.3-3.7L31 30.5Z" fill="#c79c60" fillOpacity=".12" stroke="#d0a56b" strokeWidth="1.1" />
      <path d="m31 37 2.2 6.8L40 46l-6.8 2.2L31 55l-2.2-6.8L22 46l6.8-2.2L31 37Z" fill="#cfa667" />
      <circle cx="31" cy="14" r="1.2" fill="#d0a56b" />
      <circle cx="31" cy="78" r="1.2" fill="#d0a56b" />
      <circle cx="14" cy="46" r="1.2" fill="#d0a56b" />
      <circle cx="48" cy="46" r="1.2" fill="#d0a56b" />
    </svg>
  );
}

export function SidebarLandscape(props: IconProps) {
  return (
    <svg viewBox="0 0 240 280" fill="none" aria-hidden="true" preserveAspectRatio="xMidYMax slice" {...props}>
      <defs>
        <linearGradient id="sideSky" x1="120" y1="0" x2="120" y2="280" gradientUnits="userSpaceOnUse">
          <stop stopColor="#090a1c" stopOpacity="0" />
          <stop offset=".48" stopColor="#16132c" stopOpacity=".8" />
          <stop offset="1" stopColor="#2c1c43" />
        </linearGradient>
        <linearGradient id="sideMount" x1="120" y1="125" x2="120" y2="280" gradientUnits="userSpaceOnUse">
          <stop stopColor="#43305b" />
          <stop offset="1" stopColor="#0a0b19" />
        </linearGradient>
      </defs>
      <rect width="240" height="280" fill="url(#sideSky)" />
      <circle cx="72" cy="95" r="20" fill="#efbd79" />
      <circle cx="80" cy="86" r="20" fill="#15132a" />
      <g fill="#d4ad74" opacity=".75">
        <circle cx="29" cy="48" r="1" /><circle cx="100" cy="38" r="1" /><circle cx="143" cy="74" r="1.2" /><circle cx="188" cy="47" r=".9" /><circle cx="208" cy="104" r="1" /><circle cx="45" cy="135" r=".8" /><circle cx="112" cy="105" r=".8" />
      </g>
      <path d="M0 198 44 145l29 30 38-65 37 59 27-29 65 67v73H0v-82Z" fill="url(#sideMount)" />
      <path d="M0 222 38 187l31 25 32-39 34 31 32-23 73 55v44H0v-58Z" fill="#111225" />
      <path d="M0 252c33-19 62-23 94-14 31 9 56 7 78-2 22-9 44-5 68 12v32H0v-28Z" fill="#080914" />
      <path d="M120 280c3-21 14-34 31-44 15-9 22-20 19-34" stroke="#9d7196" strokeWidth="2" opacity=".45" />
    </svg>
  );
}

export function AboutLandscape(props: IconProps) {
  return (
    <svg viewBox="0 0 760 520" fill="none" aria-hidden="true" preserveAspectRatio="xMidYMid slice" {...props}>
      <defs>
        <radialGradient id="aboutGlow" cx="0" cy="0" r="1" gradientTransform="translate(586 190) rotate(90) scale(185)" gradientUnits="userSpaceOnUse">
          <stop stopColor="#87566f" stopOpacity=".55" />
          <stop offset="1" stopColor="#17142a" stopOpacity="0" />
        </radialGradient>
        <linearGradient id="aboutMount" x1="520" y1="260" x2="520" y2="520" gradientUnits="userSpaceOnUse">
          <stop stopColor="#3c294e" />
          <stop offset="1" stopColor="#0a0b18" />
        </linearGradient>
      </defs>
      <rect width="760" height="520" fill="url(#aboutGlow)" />
      <g fill="#d5ad72" opacity=".72">
        <circle cx="410" cy="88" r="1.2" /><circle cx="501" cy="65" r="1" /><circle cx="680" cy="106" r="1.1" /><circle cx="625" cy="54" r=".8" /><circle cx="460" cy="145" r=".8" />
      </g>
      <circle cx="610" cy="168" r="48" fill="#e9b976" />
      <circle cx="629" cy="148" r="48" fill="#1a152c" />
      <path d="M300 390 430 258l54 71 62-93 76 105 54-65 84 88v156H300V390Z" fill="url(#aboutMount)" />
      <path d="M320 424 438 334l53 54 63-69 60 57 49-34 97 82v96H320v-96Z" fill="#111224" />
      <path d="M506 520c2-44 33-61 63-80 29-19 39-44 28-79" stroke="#b381a2" strokeWidth="7" opacity=".68" />
      <path d="M512 520c4-39 34-57 61-74 27-17 35-41 25-73" stroke="#d0a26b" strokeWidth="1.4" opacity=".8" />
    </svg>
  );
}

export function CelestialWheel(props: IconProps) {
  return (
    <svg viewBox="0 0 720 560" fill="none" aria-hidden="true" {...props}>
      <g stroke="currentColor" opacity=".22">
        <circle cx="360" cy="250" r="214" /><circle cx="360" cy="250" r="174" strokeDasharray="3 10" /><circle cx="360" cy="250" r="125" />
        <path d="M146 250h428M360 36v428M209 99l302 302M511 99 209 401" strokeDasharray="2 12" />
      </g>
      <g fill="currentColor" opacity=".58">
        <circle cx="188" cy="128" r="1.5" /><circle cx="541" cy="147" r="1.2" /><circle cx="227" cy="400" r="1.1" /><circle cx="493" cy="385" r="1.4" />
      </g>
      <path d="M360 20v70M325 55h70M337 32l46 46M383 32l-46 46" stroke="currentColor" strokeWidth="2" />
      <path d="m360 34 7 21 21 7-21 7-7 21-7-21-21-7 21-7 7-21Z" fill="currentColor" fillOpacity=".14" stroke="currentColor" strokeWidth="1.4" />
      <path d="M170 242v40M150 262h40M530 242v40M510 262h40" stroke="currentColor" strokeWidth="1.5" opacity=".8" />
      <path d="m170 250 4 12 12 4-12 4-4 12-4-12-12-4 12-4 4-12ZM550 250l4 12 12 4-12 4-4 12-4-12-12-4 12-4 4-12Z" stroke="currentColor" strokeWidth="1.1" />
    </svg>
  );
}

export function GenericTarotFace({ label, ...props }: IconProps & { label?: string }) {
  return (
    <svg viewBox="0 0 180 310" fill="none" aria-hidden="true" {...props}>
      <defs>
        <linearGradient id="faceBg" x1="90" y1="0" x2="90" y2="310" gradientUnits="userSpaceOnUse">
          <stop stopColor="#4e6681" /><stop offset=".48" stopColor="#d49d62" /><stop offset="1" stopColor="#2c2634" />
        </linearGradient>
      </defs>
      <rect x="2" y="2" width="176" height="306" rx="6" fill="#d7bd83" stroke="#8d7042" strokeWidth="4" />
      <rect x="9" y="9" width="162" height="292" rx="3" fill="url(#faceBg)" stroke="#292235" strokeWidth="2" />
      <circle cx="90" cy="86" r="39" fill="#e8bd68" stroke="#573c2c" strokeWidth="3" />
      <g stroke="#5e3d2d" strokeWidth="3" strokeLinecap="round">
        <path d="M90 27V8M90 164v-19M31 86H12M168 86h-19M49 45 35 31M145 141l-14-14M131 45l14-14M35 141l14-14" />
      </g>
      <path d="M72 84c5-5 11-5 16 0M92 84c5-5 11-5 16 0M84 105c4 4 8 4 12 0" stroke="#493127" strokeWidth="3" strokeLinecap="round" />
      <path d="M24 276c13-66 44-97 66-97s53 31 66 97H24Z" fill="#304054" stroke="#3c2a34" strokeWidth="3" />
      <path d="M56 268c8-37 20-58 34-58 14 0 27 21 34 58" stroke="#d39a55" strokeWidth="7" />
      <path d="M10 282h160v19H10z" fill="#e8d09b" />
      {label ? <text x="90" y="296" textAnchor="middle" fill="#241b1c" fontSize="10" fontFamily="Georgia, serif">{label.toUpperCase()}</text> : null}
    </svg>
  );
}
