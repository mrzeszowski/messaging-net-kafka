import { useState, useEffect, useRef } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { CircleUserIcon, Send, LogOut } from "lucide-react";
import axios from "axios";
import * as signalR from "@microsoft/signalr";

const apiUrl = 'http://localhost:8092';
axios.defaults.baseURL = apiUrl;

type Sender = {
  name: string;
  email: string;
};

type Message = {
  id: string;
  text: string;
  sender: Sender;
};

type User = {
  name: string;
  email: string;
};

export default function ChatUi() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState<string>("");
  const [user, setUser] = useState<User | null>(null);
  const [loginForm, setLoginForm] = useState<User>({ name: "", email: "" });

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");

    if (storedUser) {
      const user = JSON.parse(storedUser);
      setUser(user);
      fetchMessages();
      startSignalRConnection();
    }
  }, []);

  const fetchMessages = async () => {
    try {
      const response = await axios.get("/api/v1/messages");
      setMessages(response.data);
    } catch (error) {
      console.error("Error fetching messages:", error);
    }
  };

  const sendMessage = async () => {
    if (!input.trim() || !user) return;

    const request = { text: input, sender: { name: user.name, email: user.email } };

    try {
      const response = await axios.post("/api/v1/messages", request);
      const locationHeader = response.headers.location;
      const id = locationHeader.split("/").pop();
      const message = { ...request, id: id };
      setMessages([...messages, message]);
      setInput("");
    } catch (error) {
      console.error("Error sending message:", error);
    }
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      sendMessage();
    }
  };

  const handleLogin = () => {
    if (loginForm.name.trim() && loginForm.email.trim()) {
      localStorage.setItem("user", JSON.stringify(loginForm));
      setUser(loginForm);
      fetchMessages();
      startSignalRConnection();
    }
  };

  const handleLoginKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleLogin();
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  const startSignalRConnection = () => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(apiUrl + "/chat-hub")
      .withAutomaticReconnect()
      .build();

    connection.on("Subscribe", (message: Message) => {
      setMessages(prevMessages => {
        if (!prevMessages.find(msg => msg.id === message.id)) {
          return [...prevMessages, message];
        }
        return prevMessages;
      });
    });

    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch(err => console.error("Error connecting to SignalR", err));
  };

  if (!user) {
    return (
      <div className="flex flex-col h-screen items-center justify-center px-5 py-5 w-full min-h-full bg-gray-50">
        <Card className="w-full max-w-md rounded-xl shadow-lg flex flex-col">
          <CardContent className="p-6 space-y-6">
            <h2 className="text-2xl font-semibold text-center">Login</h2>
            <div className="space-y-4">
              <Input
                value={loginForm.name}
                onChange={(e) => setLoginForm({ ...loginForm, name: e.target.value })}
                placeholder="Enter your name"
                className="w-full"
                onKeyDown={handleLoginKeyDown}
              />
              <Input
                value={loginForm.email}
                onChange={(e) => setLoginForm({ ...loginForm, email: e.target.value })}
                placeholder="Enter your email"
                className="w-full"
                onKeyDown={handleLoginKeyDown}
              />
            </div>
            <Button variant="default" onClick={handleLogin} className="w-full">
              Login
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen items-center justify-center px-5 py-5 w-full min-h-full">
      <Card className="w-full h-full rounded-xl shadow-lg flex flex-col">
        <CardContent className="p-4 space-y-4 h-full flex flex-col">
          <div className="flex items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <CircleUserIcon className="w-10 h-10 rounded-full" />
              <div>
                <p className="font-semibold">{user.name}</p>
                <p className="text-gray-500 text-sm">{user.email}</p>
              </div>
            </div>
            <Button variant="destructive" onClick={handleLogout} className="flex items-center">
              <LogOut className="w-5 h-5 mr-2" />
              Logout
            </Button>
          </div>
          <div className="overflow-y-auto max-h-[calc(100%-120px)] space-y-2 flex-grow">
            {messages.map((msg) => (
              <div key={`${msg.id}`} className="mb-4">
                <div className={`flex w-max flex-col gap-2 text-xs text-gray-400 mb-2 ${msg.sender.email === user.email ? "self-end ml-auto" : ""}`}>{msg.sender.email} | 2025/02/23 16:14</div>
                <div className={`flex w-max max-w-[75%] flex-col gap-2 rounded-lg px-3 py-2 text-sm 
                ${msg.sender.email === user.email ? "bg-primary self-end ml-auto text-primary-foreground" : "bg-muted"}`}>
                  {msg.text}
                </div>
              </div>
            ))}
            <div ref={messagesEndRef} />
          </div>
          <div className="flex items-center mt-auto">
            <Input
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Type your message..."
              className="flex-1 focus:ring-0 mr-2"
            />
            <Button variant="default" size="icon" onClick={sendMessage}>
              <Send className="w-5 h-5" />
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
