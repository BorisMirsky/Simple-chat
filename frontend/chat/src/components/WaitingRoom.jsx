import { Button, Heading, Input, Text } from "@chakra-ui/react";
import { useState } from "react";


export const WaitingRoom = ({ joinChat }) => {
	const [userName, setUserName] = useState();
	const [charRoom, setChatRoom] = useState();

	const onSubmit = (e) => {
		e.preventDefault();
		joinChat(userName, charRoom);
	};

	return (

		<form
			onSubmit={onSubmit}
			className="max-w-sm w-full bg-white p-8 rounded shadow-lg"
		>
			<Heading size="lg">Online chat (Waiting room)</Heading>
			<div className="mb-4">
				<Text fontSize={"sm"}>User's name</Text>
				<Input
					name="username"
					placeholder="input your name"
					onChange={(e) => setUserName(e.target.value)}
				/>
			</div>
			<div className="mb-6">
				<Text fontSize={"sm"}>Chat's name ö</Text>
				<Input
					name="chatname"
					placeholder="input chat's name"
					onChange={(e) => setChatRoom(e.target.value)}
				/>
			</div>
			<Button type="submit" colorScheme="blue">
				join
			</Button>
		</form>
	);
};

