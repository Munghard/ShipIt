using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TaskLibrary
{
    public static Task[] tasks = new Task[]
    {
        new Task("Meeting", "Initial meeting with the team to discuss project goals", 1f, Specialty.Get("General"), 2f, null,null, "pending", 1, 0f),
        new Task("Research", "Investigate technologies, tools, or domain-specific knowledge", 2f, Specialty.Get("General"), 3f, null,null, "pending", 2, 0f),
        new Task("Planning", "Outline project scope, timeline, and resource allocation", 2f, Specialty.Get("Management"), 4f, null, null, "pending", 3, 0f),
        new Task("Prototype", "Create a basic prototype to validate ideas and architecture", 3f, Specialty.Get("UI"), 5f, null, null, "pending", 4, 0f),
        new Task("System Design", "Define the architecture and overall structure of the application", 3f, Specialty.Get("Coding"), 6f, null, null, "pending", 5, 0f),
        new Task("Coding", "Develop core functionality and implement features", 4f, Specialty.Get("Coding"), 20f, null, null,   "pending", 6, 0f),
        new Task("Code Review", "Review code for quality, style, and bugs", 2f, Specialty.Get("Coding"), 3f, null,  null, "pending", 7, 0f),
        new Task("UI", "Design and integrate the user interface", 3f, Specialty.Get("UI"), 10f, null, null, "pending", 8, 0f),
        new Task("Content Integration", "Add and format text, media, and other content", 2f, Specialty.Get("UI"), 4f, null, null, "pending", 9, 0f),
        new Task("Testing", "Run unit tests and debug the application", 3f, Specialty.Get("Testing"), 8f, null, null, "pending", 10, 0f),
        new Task("Performance Optimization", "Improve speed and resource usage", 4f, Specialty.Get("Coding"), 5f, null, null, "pending", 11, 0f),
        new Task("Security Review", "Ensure the application is secure against common vulnerabilities", 3f, Specialty.Get("Testing"), 3f, null, null, "pending", 12, 0f),
        new Task("Deployment Prep", "Set up environment and finalize release checklist", 2f, Specialty.Get("Deployment"), 2f, null, null, "pending", 13, 0f),
        new Task("Deployment", "Prepare and release the final build to production", 2f, Specialty.Get("Deployment"), 3f, null, null, "pending", 14, 0f),
        new Task("Post-Launch Monitoring", "Monitor app stability and respond to user feedback", 2f, Specialty.Get("Management"), 4f, null, null, "pending", 15 , 0f),

    };

    public static Task[] funTasks = new Task[]
    {
        // Fun tasks
        new Task("Fix the Printer", "Why is it *always* the printer?", 1f, Specialty.Get("General"), 2f, null, null, "pending", 16  , 0f),
        new Task("Team Coffee Break", "Mandatory morale-boosting caffeine session", 0.5f, Specialty.Get("General"), 1f, null, null, "pending", 17   , 0f),
        new Task("Stand-up Meeting", "Discuss what everyone did yesterday, today, and forever", 1f, Specialty.Get("Management"), 1f, null, null, "pending", 18  , 0f),
        new Task("Refactor That One Function", "You know the one. Everyone avoids it.", 3f, Specialty.Get("Coding"), 4f, null, null, "pending", 19, 0f),
        new Task("Write Documentation", "The greatest fiction ever written", 2f, Specialty.Get("General"), 3f, null,null, "pending", 20, 0f),
        new Task("Merge Conflict Resolution", "Two branches enter. One branch leaves.", 3f, Specialty.Get("Coding"), 2f, null,null, "pending", 21, 0f),
        new Task("Office Nerf War", "Critical team-building exercise", 0.5f, Specialty.Get("General"), 2f, null, null, "pending", 22, 0f),
        new Task("Keyboard Maintenance", "Sticky keys and mysterious crumbs", 1f, Specialty.Get("UI"), 1f, null, null, "pending", 23, 0f),
        new Task("Monitor Cable Hunt", "A rite of passage for new employees", 1f, Specialty.Get("General"), 2f, null, null, "pending", 24, 0f),
        new Task("Deploy on Friday", "What could go wrong?", 4f, Specialty.Get("Deployment"), 1f, null, null, "pending", 25, 0f),
        // Add more tasks as needed
    };
    public static Task[] GetMainTasks() { return tasks; } 
}
