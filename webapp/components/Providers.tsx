"use client";

import { HeroUIProvider } from "@heroui/react";
import { ReactNode } from "react";

export default function Providers({ children }: { children: ReactNode }) {
  return (
    <HeroUIProvider className={"h-full flex flex-col"}>
      {children}
    </HeroUIProvider>
  );
}
