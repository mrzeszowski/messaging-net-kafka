import { JSX } from "react";
import ChatUi from "./ChatUi";

export default function App(): JSX.Element {
  return (
    <div className="h-screen flex items-center justify-center">
      <ChatUi />
    </div>
  );
}