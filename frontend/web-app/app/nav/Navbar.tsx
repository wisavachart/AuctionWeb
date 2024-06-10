"use client";
const Navbar = () => {
  console.log("Client component");
  return (
    <header className="sticky top-0 z-50 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md ">
      <div>
        <div>Carsties Auctions</div>
      </div>
      <div>search</div>
      <div>login</div>
    </header>
  );
};

export default Navbar;
