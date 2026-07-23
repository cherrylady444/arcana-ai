import type { SVGProps } from "react";

type IconProps = SVGProps<SVGSVGElement>;

export function BrandMarkIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" {...props}>
      <path
        d="M16 2v28M2 16h28M6.2 6.2l19.6 19.6M25.8 6.2 6.2 25.8"
        stroke="currentColor"
        strokeWidth="1.25"
        strokeLinecap="round"
      />

      <path
        d="m16 8 2.2 5.8L24 16l-5.8 2.2L16 24l-2.2-5.8L8 16l5.8-2.2L16 8Z"
        fill="currentColor"
        fillOpacity=".2"
        stroke="currentColor"
        strokeWidth="1.2"
      />

      <circle cx="16" cy="3.5" r="1.2" fill="currentColor" />
      <circle cx="28.5" cy="16" r="1.2" fill="currentColor" />
      <circle cx="16" cy="28.5" r="1.2" fill="currentColor" />
      <circle cx="3.5" cy="16" r="1.2" fill="currentColor" />
    </svg>
  );
}

export function NewReadingIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path
        d="M12 2.8c.7 4.8 2.7 7 7 7.7-4.3.8-6.3 3-7 7.7-.7-4.7-2.7-6.9-7-7.7 4.3-.7 6.3-2.9 7-7.7Z"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinejoin="round"
      />

      <path
        d="M19 3.5c.2 1.5.9 2.2 2.2 2.5-1.3.3-2 1-2.2 2.5-.2-1.5-.9-2.2-2.2-2.5 1.3-.3 2-1 2.2-2.5Z"
        fill="currentColor"
      />
    </svg>
  );
}

/**
 * Represents the reusable predefined spread templates.
 * It uses two overlapping tarot cards so it is visually distinct
 * from the Readings history icon.
 */
export function SpreadsIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <rect
        x="5"
        y="4"
        width="11"
        height="15"
        rx="2"
        stroke="currentColor"
        strokeWidth="1.6"
      />

      <path
        d="M9 4.5 17.2 3a2 2 0 0 1 2.3 1.6l2 10.7a2 2 0 0 1-1.6 2.3L16 18.3"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
      />

      <path
        d="M10.5 8.2c.3 1.9 1.1 2.8 2.8 3.1-1.7.3-2.5 1.2-2.8 3.1-.3-1.9-1.1-2.8-2.8-3.1 1.7-.3 2.5-1.2 2.8-3.1Z"
        stroke="currentColor"
        strokeWidth="1.25"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export function ReadingsIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path
        d="M4 5.5c2.7-.9 5.3-.5 8 1.1v12c-2.7-1.6-5.3-2-8-1.1v-12Z"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinejoin="round"
      />

      <path
        d="M20 5.5c-2.7-.9-5.3-.5-8 1.1v12c2.7-1.6 5.3-2 8-1.1v-12ZM7 9h2.5M7 12h2.5M14.5 9H17M14.5 12H17"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export function AboutIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <circle
        cx="12"
        cy="12"
        r="8.4"
        stroke="currentColor"
        strokeWidth="1.7"
      />

      <path
        d="M12 10.7v5.1M12 7.8h.01"
        stroke="currentColor"
        strokeWidth="1.9"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function SearchIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <circle
        cx="10.8"
        cy="10.8"
        r="6.1"
        stroke="currentColor"
        strokeWidth="1.8"
      />

      <path
        d="m15.4 15.4 4 4"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function ChevronDownIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path
        d="m7 9.5 5 5 5-5"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export function ChevronRightIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path
        d="m9.5 6 6 6-6 6"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export function MenuIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path
        d="M4 7h16M4 12h16M4 17h16"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function CloseIcon(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" {...props}>
      <path
        d="m6 6 12 12M18 6 6 18"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function TarotCardBack(props: IconProps) {
  return (
    <svg viewBox="0 0 52 76" fill="none" aria-hidden="true" {...props}>
      <rect
        x="1"
        y="1"
        width="50"
        height="74"
        rx="6"
        fill="#0c0d1c"
        stroke="#9b794d"
        strokeWidth="1.5"
      />

      <rect
        x="5"
        y="5"
        width="42"
        height="66"
        rx="4"
        stroke="#5e4934"
        strokeWidth="1"
      />

      <path
        d="M26 14v48M12 38h28M17 22l18 32M35 22 17 54"
        stroke="#a88655"
        strokeWidth=".8"
        strokeLinecap="round"
        opacity=".55"
      />

      <path
        d="m26 25 3.1 9.1L38 38l-8.9 3.9L26 51l-3.1-9.1L14 38l8.9-3.9L26 25Z"
        fill="#c9a56e"
        fillOpacity=".12"
        stroke="#d2b074"
        strokeWidth="1.1"
      />

      <circle cx="26" cy="14" r="1.2" fill="#d2b074" />
      <circle cx="26" cy="62" r="1.2" fill="#d2b074" />
      <circle cx="12" cy="38" r="1.2" fill="#d2b074" />
      <circle cx="40" cy="38" r="1.2" fill="#d2b074" />
    </svg>
  );
}

export function SidebarLandscape(props: IconProps) {
  return (
    <svg
      viewBox="0 0 220 220"
      fill="none"
      aria-hidden="true"
      preserveAspectRatio="xMidYMax slice"
      {...props}
    >
      <defs>
        <linearGradient
          id="sidebar-sky"
          x1="110"
          y1="0"
          x2="110"
          y2="220"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#0c0d20" stopOpacity="0" />
          <stop offset=".55" stopColor="#191534" stopOpacity=".75" />
          <stop offset="1" stopColor="#2b1b42" />
        </linearGradient>

        <linearGradient
          id="sidebar-mountain"
          x1="110"
          y1="95"
          x2="110"
          y2="220"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#34264b" />
          <stop offset="1" stopColor="#0b0c1a" />
        </linearGradient>
      </defs>

      <rect width="220" height="220" fill="url(#sidebar-sky)" />

      <circle cx="63" cy="84" r="18" fill="#e6b779" />
      <circle cx="70" cy="77" r="18" fill="#15132a" />

      <g fill="#d8b67a" opacity=".8">
        <circle cx="27" cy="42" r="1" />
        <circle cx="91" cy="35" r="1" />
        <circle cx="129" cy="62" r="1.2" />
        <circle cx="172" cy="43" r=".8" />
        <circle cx="190" cy="89" r="1" />
        <circle cx="41" cy="116" r=".8" />
        <circle cx="102" cy="92" r=".8" />
      </g>

      <path
        d="M0 163 40 122l27 25 35-52 34 48 24-24 60 54v47H0v-57Z"
        fill="url(#sidebar-mountain)"
      />

      <path
        d="M0 180 34 153l28 20 29-31 31 25 29-18 69 45v26H0v-40Z"
        fill="#111224"
      />

      <path
        d="M0 201c30-15 57-18 86-11 29 7 52 6 72-1 20-7 41-4 62 10v21H0v-19Z"
        fill="#090a16"
      />
    </svg>
  );
}